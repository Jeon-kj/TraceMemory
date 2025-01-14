using UnityEngine;
using UnityEngine.UI;
using Firebase.Storage;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.Collections;
using System;
using Firebase.Database;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using System.Globalization;

public class Loader : MonoBehaviourPunCallbacks
{
    private DatabaseReference databaseReference;

    Uploader uploader;
    CanvasManager canvasManager;
    AlwaysOnCanvas alwaysOnCanvas;

    private void Awake()
    {
        uploader = GetComponent<Uploader>();
        canvasManager = FindObjectOfType<CanvasManager>();
        alwaysOnCanvas = canvasManager.AlwaysOnCanvas.GetComponent<AlwaysOnCanvas>();
    }

    async void Start()
    {
        await FirebaseManager.Instance.WaitForFirebaseInitialized();
        databaseReference = FirebaseManager.Instance.database.RootReference;
    }

    public void LoadPlayerImage(Image targetImage, string fileName)
    {
        // Firebase���� URL ��������
        uploader.GetImageUrl(fileName, (imageUrl) =>
        {
            // Firebase�κ��� ������ URL�� ����Ͽ� �̹����� �ε�
            UnityMainThreadDispatcher.Enqueue(() =>
            {
                // URL�� �޾ƿ� �� �̹����� �ε�
                LoadImageFromUrl(targetImage, imageUrl);
            });
        });
    }

    // URL���� �̹����� �ε��ϴ� �޼���
    public void LoadImageFromUrl(Image targetImage, string imageUrl)
    {
        try
        {
            StartCoroutine(DownloadImage(targetImage, imageUrl));
        }
        catch (Exception ex)
        {
            Debug.LogError("An error occurred: " + ex.Message);
        }
    }

    // �ڷ�ƾ�� ���� URL���� �̹����� �ٿ�ε��ϰ� UI�� ����
    private IEnumerator DownloadImage(Image targetImage, string imageUrl)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);

            // UI �۾��� ���� ������� ����
            targetImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
    }

    // �� �ο��� ����
    public async Task<int> LoadPlayerMaxNumber()  // UploadPlayerMaxNumber(n)
    {
        var roomRef = databaseReference.Child(GameManager.Instance.GetRoomCode()).Child("PlayerMaxNumber");

        // ���� �񵿱������� �����ɴϴ�.
        var snapshot = await roomRef.GetValueAsync();
        if (snapshot.Exists)
        {
            int playerMaxNumber = int.Parse(snapshot.Value.ToString());
            return playerMaxNumber;
        }
        else
        {
            Debug.LogError("Failed to load PlayerMaxNumber: Snapshot does not exist.");
            return -1;  // ���� �Ǵ� �������� �ʴ� ��쿡 ���� �⺻��.
        }
    }

    // ������ ������ QuestionCanvas�� ���� �� �ҷ�����.
    public void LoadAnswers(int actorNumber, Action<int[]> onAnswersLoaded)
    {
        string roomCode = GameManager.Instance.GetRoomCode();
        // Firebase ��ο��� ActorNumber�� �����͸� ������
        databaseReference.Child(roomCode).Child("user_answers").Child(actorNumber.ToString()).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                DataSnapshot snapshot = task.Result;
                List<int> answersList = new List<int>();

                foreach (var child in snapshot.Children)
                {
                    int answer = int.Parse(child.Value.ToString());
                    answersList.Add(answer);
                }

                // �ҷ��� answers �迭�� �ݹ����� ����
                onAnswersLoaded(answersList.ToArray());
            }
            else
            {
                Debug.LogError("Error loading answers: " + task.Exception);
            }
        });
    }

    /*public void LoadFirstImpressionScore(string scoreType, int targetActorNumber, Action<int> onFirstImpressionScoreLoaded)
    {
        string roomCode = GameManager.Instance.GetRoomCode();
        // Firebase ��ο��� ActorNumber�� �����͸� ������
        databaseReference.Child(roomCode).Child(targetActorNumber.ToString()).Child(scoreType).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                if (task.Result.Exists)
                {
                    int firstImpressionScore = int.Parse(task.Result.Value.ToString());

                    onFirstImpressionScoreLoaded(firstImpressionScore);
                }
                else
                {
                    Debug.LogError("FirstImpressionScore does not exist for ActorNumber: " + targetActorNumber);
                    onFirstImpressionScoreLoaded(0);  // ���� ���� ��� �⺻�� 0�� ����
                }
            }
            else
            {
                Debug.LogError($"Error loading {scoreType}: " + task.Exception);
            }
        });
    }*/



    // About Message (In AlwaysOnCanvas)
    public void LoadAllMessages(DatabaseReference messageRef, Action onMessagesLoaded)
    {
        messageRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                if (snapshot.Exists && snapshot.HasChildren)
                {
                    foreach (DataSnapshot childSnapshot in snapshot.Children)
                    {
                        string message = childSnapshot.Value.ToString();
                        alwaysOnCanvas.AddMessageToContent(message);
                    }
                }
                // �޽����� ��� �ҷ������Ƿ� �ݹ� ����
                onMessagesLoaded?.Invoke();
            }
            else
            {
                Debug.LogError($"Failed to load messages from Firebase: {task.Exception}");
            }
        });
    }

    public void HandleNewMessageAdded(object sender, ChildChangedEventArgs e)
    {
        if (e.Snapshot.Exists && e.Snapshot.Value != null)
        {
            string newMessage = e.Snapshot.Value.ToString();
            Debug.Log($"{e.Snapshot.Key} Message updated: {newMessage}");
            alwaysOnCanvas.AddMessageToContent(newMessage);
        }
        else
        {
            Debug.LogError("Snapshot does not exist or is null.");
        }
    }

    /*public void EnsureSecretMessagePath(int targetActorNumber, Action onPathCreated)
    {
        string roomCode = GameManager.Instance.GetRoomCode();

        // Firebase���� ��� ����
        DatabaseReference messageRef = databaseReference
            .Child(roomCode)
            .Child(targetActorNumber.ToString())
            .Child("SecretMessage");

        // ��ΰ� �����ϴ��� Ȯ��
        messageRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                if (!snapshot.Exists)
                {
                    // ��ΰ� �������� ������ �⺻���� �����ؼ� ��θ� ����
                    messageRef.SetValueAsync("Initial Message").ContinueWith(setTask =>
                    {
                        if (setTask.IsCompleted)
                        {
                            Debug.Log("SecretMessage path created with initial message.");
                            onPathCreated?.Invoke(); // ��ΰ� �����Ǿ����� �˸�
                        }
                        else
                        {
                            Debug.LogError($"Failed to create SecretMessage path: {setTask.Exception}");
                        }
                    });
                }
                else
                {
                    // ��ΰ� �̹� �����ϸ� �ݹ��� �ٷ� ȣ��
                    onPathCreated?.Invoke();
                }
            }
            else
            {
                Debug.LogError($"Failed to check SecretMessage path: {task.Exception}");
            }
        });
    }*/



    // About LoveCard (In AlwaysOnCanvas)
    public void HandleScoreChanged(object sender, ValueChangedEventArgs e, int targetActorNumber)
    {
        if (e.Snapshot.Exists && e.Snapshot.Value != null)
        {
            int updatedScore = int.Parse(e.Snapshot.Value.ToString());
            Debug.Log($"{e.Snapshot.Key} score changed: {updatedScore}");

            // ���� ������Ʈ�� ���� �߰� ����
            alwaysOnCanvas.OnScoreChanged(targetActorNumber, updatedScore);
        }
        else
        {
            Debug.LogWarning("Score data does not exist or is null.");
        }
    }

    /*public void EnsureLoveCardScorePath(int targetActorNumber, Action onPathCreated)
    {
        string roomCode = GameManager.Instance.GetRoomCode();

        // Firebase���� ���� ��� ����
        DatabaseReference scoreRef = databaseReference
            .Child(roomCode)
            .Child(targetActorNumber.ToString())
            .Child("LoveCardScore");

        // ��ΰ� �����ϴ��� Ȯ��
        scoreRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                if (!snapshot.Exists)
                {
                    // ��ΰ� �������� ������ �⺻���� �����ؼ� ��θ� ����
                    scoreRef.SetValueAsync(0).ContinueWith(setTask => // ������ 0���� �ʱ�ȭ
                    {
                        if (setTask.IsCompleted)
                        {
                            Debug.Log("LoveCardScore path created with initial score.");
                            onPathCreated?.Invoke(); // ��ΰ� �����Ǿ����� �˸�
                        }
                        else
                        {
                            Debug.LogError($"Failed to create LoveCardScore path: {setTask.Exception}");
                        }
                    });
                }
                else
                {
                    // ��ΰ� �̹� �����ϸ� �ݹ��� �ٷ� ȣ��
                    onPathCreated?.Invoke();
                }
            }
            else
            {
                Debug.LogError($"Failed to check LoveCardScore path: {task.Exception}");
            }
        });
    }*/

    public void EnsurePath(int targetActorNumber, string type, Action onPathCreated)
    {
        string roomCode = GameManager.Instance.GetRoomCode();

        // Firebase���� ���� ��� ����
        DatabaseReference newRef = databaseReference
            .Child(roomCode)
            .Child(targetActorNumber.ToString())
            .Child(type);

        // ��ΰ� �����ϴ��� Ȯ��
        newRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                if (!snapshot.Exists)
                {
                    // ��ΰ� �������� ������ �⺻���� �����ؼ� ��θ� ����
                    newRef.SetValueAsync(type=="LoveCardScore" ? 0 : "Initial Message").ContinueWith(setTask => // ������ 0���� �ʱ�ȭ
                    {
                        if (setTask.IsCompleted)
                        {
                            Debug.Log($"{type} path created.");
                            onPathCreated?.Invoke(); // ��ΰ� �����Ǿ����� �˸�
                        }
                        else
                        {
                            Debug.LogError($"Failed to create {type} path: {setTask.Exception}");
                        }
                    });
                }
                else
                {
                    // ��ΰ� �̹� �����ϸ� �ݹ��� �ٷ� ȣ��
                    //Debug.Log($"{type} path already exist.");
                    onPathCreated?.Invoke();
                }
            }
            else
            {
                Debug.LogError($"Failed to check {type} path: {task.Exception}");
            }
        });
    }

    // All Player Ready?
    public void CheckAndNotifyEndOfReady(string type)
    {
        // roomCode.MiniGame1.VotedCount �ش� ��ο�, ��ǥ�� ������ �ο��� ���� ��ü �÷��̾� ���� �Ȱ����� RPCó��.
        // gameType <= {"MiniGame1", "MiniGame2"}
        var roomRef = databaseReference.Child(GameManager.Instance.GetRoomCode()).Child(type).Child("ReadyCount");
        roomRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                int currentCount = int.Parse(snapshot.Value.ToString());
                if (currentCount == GameManager.Instance.GetPlayerMaxNumber())
                {
                    UnityMainThreadDispatcher.Enqueue(() =>
                    {
                        if (photonView == null)
                        {
                            Debug.LogError("PhotonView is not assigned!");
                            return;
                        }

                        // �ڵ带 ��Ȱ���ϱ� ���� ������ 0���� �ʱ�ȭ.
                        uploader.InitReadyCount(type);

                        foreach (Player player in PhotonNetwork.PlayerList)
                        {                        
                            try
                            {
                                photonView.RPC("ReadyToStart", player, type);
                            }
                            catch (Exception e)
                            {
                                Debug.LogError($"Error: {e.Message}");
                            }
                        }
                    });

                }
            }
        });
    }

    [PunRPC]
    void ReadyToStart(string type)
    {
        if (type == "MiniGame1")
        {
            canvasManager.MiniGame1.GetComponent<MiniGame1>().StartMiniGame();
        }
        else if (type == "MiniGame2")
        {
            canvasManager.MiniGame2.GetComponent<MiniGame2>().StartMiniGame();
        }
        else if(type == "MiniGame1Timer" || type == "MiniGame2Timer")
        {
            canvasManager.AuxiliaryCanvas.GetComponent<AuxiliaryCanvas>().SetTimerSign(false);
        }
    }

    // About MiniGame
    public async Task<(List<int> topScorers, int highestScore)> FindTopScorers(string gameType) // Uploader.UploadReceivedVotesMG1(targetActorNumber)
    {
        // roomCode.gameType2.ActorNumber.ReceivedVotes �ش� ��ο��� ���� ���� ��ǥ�� ���� �÷��̾��� ������ ActorNumber�� ��ȯ
        // gameType <= {"MiniGame1", "MiniGame2"}
        Debug.Log($"Fetching top scorers for game type: {gameType}");
        string roomCode = GameManager.Instance.GetRoomCode();

        int highestScore = 0;
        List<int> topScorers = new List<int>();

        foreach (var one in PhotonNetwork.PlayerList)
        {
            var scoreRef = databaseReference.Child(roomCode).Child(gameType).Child(one.ActorNumber.ToString()).Child("ReceivedVotes");
            DataSnapshot snapshot = await scoreRef.GetValueAsync();
            Debug.Log($"scoreRef : {scoreRef}");

            if (snapshot.Exists)
            {
                int score = int.Parse(snapshot.Value.ToString());
                if (score > highestScore)
                {
                    highestScore = score;
                    topScorers.Clear();
                    topScorers.Add(one.ActorNumber);
                }
                else if (score == highestScore)
                {
                    topScorers.Add(one.ActorNumber);
                }
            }
        }
        Debug.Log($"topScorers : {topScorers}");
        Debug.Log($"highestScore : {highestScore}");
        return (topScorers, highestScore);
    }

    public async Task<List<int>> FindTopScorerPredictors(string gameType, List<int> topScorer) // Uploader.UploadSelectionMG1(targetActorNumber)
    {
        // roomCode.gameType.ActorNumber.Selection �ش� ��ο� TopScorers�� ActorNumber�� ���� ���� ���� �÷��̾� ã�Ƴ�.
        // gameType <= {"MiniGame1", "MiniGame2"}
        Debug.Log($"Fetching top scorer predictors for game type: {gameType}");
        string roomCode = GameManager.Instance.GetRoomCode();

        List<int> predictors = new List<int>();

        foreach (var one in PhotonNetwork.PlayerList)
        {
            var selectionRef = databaseReference.Child(roomCode).Child(gameType).Child(one.ActorNumber.ToString()).Child("Selection");
            DataSnapshot snapshot = await selectionRef.GetValueAsync();
            Debug.Log($"selectionRef : {selectionRef}");

            if (snapshot.Exists)
            {
                int predictedTopScorer = int.Parse(snapshot.Value.ToString());

                if (topScorer.Exists(x => x == predictedTopScorer))
                {
                    predictors.Add(one.ActorNumber);
                    Debug.Log($"dictors.Add(one.ActorNumber) {one.ActorNumber}");
                }
                    
            }
        }

        return predictors;
    }

    public void CheckAndNotifyEndOfVoting(string gameType)  // UploadVotedCount(gameType)
    {
        // roomCode.MiniGame1.VotedCount �ش� ��ο�, ��ǥ�� ������ �ο��� ���� ��ü �÷��̾� ���� �Ȱ����� RPCó��.
        // gameType <= {"MiniGame1", "MiniGame2"}
        Debug.Log("CheckAndNotifyEndOfVoting check in task Is Completed");
        var roomRef = databaseReference.Child(GameManager.Instance.GetRoomCode()).Child(gameType).Child("VotedCount");

        roomRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                int currentCount = int.Parse(snapshot.Value.ToString());
                Debug.Log($"currentCount : {currentCount}");
                if (currentCount == GameManager.Instance.GetPlayerMaxNumber())
                {

                    UnityMainThreadDispatcher.Enqueue(() =>
                    {
                        if (photonView == null)
                        {
                            Debug.LogError("PhotonView is not assigned!");
                            return;
                        }
                        
                        // �ڵ带 ��Ȱ���ϱ� ���� ������ 0���� �ʱ�ȭ.
                        try
                        {
                            roomRef.SetValueAsync(0).ContinueWith(setTask =>
                            {
                                if (setTask.IsFaulted)
                                {
                                    Debug.LogError($"Failed to set value in Firebase: {setTask.Exception}");
                                }
                            });
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Error Find {e}");
                        }

                        foreach (Player player in PhotonNetwork.PlayerList)
                        {
                            try
                            {                                
                                photonView.RPC("ReceiveVotingEndSign", player, gameType);
                            }
                            catch (Exception e)
                            {
                                Debug.LogError($"Error: {e.Message}");
                            }
                        }                        
                    });
                    
                }
            }
        });
    }

    public async Task<(int leastCountChoiceIdx, List<int> players)> FindChoiceAndPlayer()   // UploadPlayerChoiceMG2(index)
    {
        // roomCode.MiniGame2.index �ش� ��ο��� �������� ������ �÷��̾��� ���� ��.
        // ���� ���� ���õ� �������� �ε����� �ش� �������� ������ �÷��̾���� ��ȯ��.
        // index <= {0,1}
        Debug.Log("FindLeastCountChoice Started");

        string roomCode = GameManager.Instance.GetRoomCode();

        int leastCountChoiceIdx = -1;
        List<int> players = new List<int>();
        int leastCount = -1;

        for(int i=0; i<2; i++)
        {
            var selectionRef = databaseReference.Child(roomCode).Child("MiniGame2").Child(i.ToString());
            DataSnapshot snapshot = await selectionRef.GetValueAsync();

            if (snapshot.Exists)
            {
                var data = snapshot.Value as List<object>;

                if (data != null)
                {
                    int dataCount = data.Count;
                    if (leastCount == -1 || leastCount > dataCount)
                    {
                        leastCount = dataCount;                        
                        leastCountChoiceIdx = i;
                    }

                    if (leastCountChoiceIdx != i) continue;
                    else players.Clear();

                    foreach (var item in data)
                    {
                        string actorNumber = item.ToString();
                        players.Add(int.Parse(actorNumber));
                        Debug.Log($"ActorNumber: {actorNumber}");
                    }
                }
                else
                {
                    // �ƹ��� �������� ���� �������� ����.
                    leastCount = 0;
                    leastCountChoiceIdx = i;
                    players.Clear();
                }
            }
        }

        if (leastCount == GameManager.Instance.GetPlayerMaxNumber() / 2)
        {
            // ���� ó��.
        }
        Debug.Log($"leastCountChoiceIdx : {leastCountChoiceIdx}, players : {players}");
        return (leastCountChoiceIdx, players);
    }

    void SelectSignMG1()
    {
        Debug.Log("SelectSignMG1 started");

        MiniGame1 miniGame1 = canvasManager.MiniGame1.GetComponent<MiniGame1>();
        string sign = miniGame1.GetSign();

        Debug.Log($"Before SIGN :: {sign}");
        if (sign == "")
        {
            miniGame1.SetSign("ProcessTopScorers");
            sign = miniGame1.GetSign();
        }
        else if (sign == "ProcessTopScorers")
        {
            miniGame1.SetSign("ProcessTopPredictors");
            sign = miniGame1.GetSign();
        }
        Debug.Log($"After SIGN :: {sign}");
    }

    [PunRPC]
    void ReceiveVotingEndSign(string gameType)
    {
        if (!canvasManager || !canvasManager.MiniGame1)
        {
            Debug.LogError("CanvasManager or MiniGame1 component is not initialized!");
            return;
        }

        if (gameType == "MiniGame1")
        {
            SelectSignMG1();
            canvasManager.MiniGame1.GetComponent<MiniGame1>().OnAllPlayersVoted();
        }
        else if (gameType == "MiniGame2")
        {
            canvasManager.MiniGame2.GetComponent<MiniGame2>().OnAllPlayersSelected();
        }
               
    }

    //About Timer
    public void GetStartTime()
    {
        string roomCode = GameManager.Instance.GetRoomCode();

        DatabaseReference timerRef = databaseReference
            .Child(roomCode)
            .Child("StartTime");

        timerRef.GetValueAsync().ContinueWith(task => {
            if (task.IsFaulted)
            {
                Debug.LogError("Error reading timer start time: " + task.Exception);
            }
            else if (task.Result.Value != null)
            {
                DateTime startTime = DateTime.Parse(task.Result.Value.ToString());

                UnityMainThreadDispatcher.Enqueue(() =>
                {
                    AuxiliaryCanvas auxiliaryCanvas = canvasManager.AuxiliaryCanvas.GetComponent<AuxiliaryCanvas>();
                    auxiliaryCanvas.timer.SetActive(true);
                    auxiliaryCanvas.InitializeLocalTimer(startTime, 60); // Ÿ�̸Ӹ� 60�ʷ� ����    
                });
            }
        });
    }
}

