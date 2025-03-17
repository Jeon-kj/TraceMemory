using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Storage;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;

    public FirebaseAuth auth;
    public FirebaseDatabase database;
    public FirebaseStorage storage;

    private TaskCompletionSource<bool> firebaseInitialized = new TaskCompletionSource<bool>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        InitializeFirebase();
    }

    void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            auth = FirebaseAuth.DefaultInstance;
            try
            {
                database = FirebaseDatabase.GetInstance(app, "https://tracememory-daa32-default-rtdb.asia-southeast1.firebasedatabase.app/");
                Debug.Log("Firebase Database initialized successfully");
            }
            catch (System.Exception ex)
            {
                ErrorCanvas.Instance.ShowErrorMessage("Firebase Database initialization failed", () => {
                    Debug.LogError("Firebase Database initialization failed: " + ex.Message);
                });
            }

            storage = FirebaseStorage.DefaultInstance;
            Debug.Log("Firebase initialized");
            firebaseInitialized.SetResult(true); // Firebase 초기화 완료
        });
    }

    public Task WaitForFirebaseInitialized()
    {
        return firebaseInitialized.Task;
    }
}
