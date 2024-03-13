using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using Unity.VisualScripting;
using System.Threading.Tasks;

public class Realtime : MonoBehaviour
{
    // conexion con Firebase (_app(para decir que es privada))
    private FirebaseApp _app;
    // Singleton de la Base de Datos
    private FirebaseDatabase _db;
    // referencia a la 'coleccion' Clientes
    private DatabaseReference _refClientes;
    // GameObject a modificar (el jugador en este caso)
    public GameObject ondavital;
    // GameObject que servira para crear los pickUps
    public GameObject pickUp;
    // referencia a la colección de los pickUps
    private DatabaseReference _refPickUp;
    // referencia al usuarioConectado
    private string _userId;
    // referencia base de datos del usuario
    private DatabaseReference _refUser;
    private bool _refUserCreated = false;
    
    // Start is called before the first frame update
    void Start() {
        // realizamos la conexion a Firebase
        _app = Conexion();
        
        // obtenemos el Singleton de la base de datos
        _db = FirebaseDatabase.DefaultInstance;
        
        // Obtenemos la referencia a TODA la base de datos
        // DatabaseReference reference = db.RootReference;
        
        // Definimos la referencia a Clientes
        _refClientes = _db.GetReference("Jugadores");

        // Definimos la referencia a PickUps
        _refPickUp = _db.GetReference("Prefabs");

        // Recogemos todos los valoresd de las posiciones del prefab y las creamos
        _refPickUp.GetValueAsync().ContinueWithOnMainThread(task => {
                if(task.IsFaulted) {
                    // error
                } else if(task.IsCompleted){
                    DataSnapshot snapshot = task.Result;
                    AñadirPrefabs(snapshot);
                }
            }
        );

        // Añadimos al jugador
        AltaDevice();

        // recogemos la referencia del usuario
        _refUser = _db.GetReference("Jugadores").Child(_userId);
    }
    
    // realizamos la conexion a Firebase
    // devolvemos una instancia de esta aplicacion
    FirebaseApp Conexion()
    {
        FirebaseApp firebaseApp = null;
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                firebaseApp = FirebaseApp.DefaultInstance;
                // Set a flag here to indicate whether Firebase is ready to use by your app.
            }
            else
            {
                Debug.LogError(System.String.Format(
                    "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
                firebaseApp = null;
            }
        });
            
        return firebaseApp;
    }

    void AñadirPrefabs(DataSnapshot snapshot){
        // recorremos cada prefab
        foreach(var resultado in snapshot.Children) {
            // inicializamos las variables de las posiciones
            var x = 0f;
            var y = 0f;
            var z = 0f;
            Debug.LogFormat("Key = {0}", resultado.Key);
            // recorremos cada posicion del prefab
            foreach(var item in resultado.Children) {
                Debug.LogFormat("(key){0}:(value){1}", item.Key, item.Value);
                if(item.Key == "x"){
                    float.TryParse(item.Value.ToString(), out x);
                    Debug.LogFormat("MI XXXXXXXXXXXXX =  {0}", x);
                } else if(item.Key == "y"){
                    float.TryParse(item.Value.ToString(), out y);
                    Debug.LogFormat("MI YYYYYYYYYYYYY = {0}", y);
                } else if(item.Key == "z") {
                    float.TryParse(item.Value.ToString(), out z);
                    Debug.LogFormat("MI ZZZZZZZZZZZ = {0}", z);
                } else{
                    Debug.LogFormat("aaaaaaaaaaaa");
                }
            }
            // creamos la posicion en la que estara el pickUp
            Vector3 spawnPosition = new Vector3(x,y,z);
            // instanciamos el pickUp en la posicion
            // prefab/objeto - Vector3 - Rotation
            Instantiate(pickUp, spawnPosition, pickUp.transform.rotation);
        }
    }


    // doy de alta un nodo con un identificador unico
    async void AltaDevice() {
        // recogemos el id del equipo
        _userId = SystemInfo.deviceUniqueIdentifier;
        // miramos si existe el jugador con ese id
        DataSnapshot snapshot = await _refClientes.Child(_userId).GetValueAsync();
        if(!snapshot.Exists){
            // si no existe lo crea con los siguientes datos
            _refClientes.Child(_userId).Child("nombre").SetValueAsync("Mi dispositivo");
            _refClientes.Child(_userId).Child("Puntos").SetValueAsync(0);
            _refClientes.Child(_userId).Child("Record").SetValueAsync(0);
        }
        // Añadimos la posición con valores iniciales
        Dictionary<string, object> posicion = new Dictionary<string, object>();
        posicion["x"] = 0; // Valor inicial de x
        posicion["y"] = 0; // Valor inicial de y
        posicion["z"] = 0; // Valor inicial de z
        _refClientes.Child(_userId).Child("posicion").SetValueAsync(posicion);
        _refClientes.Child(_userId).Child("connected").SetValueAsync(true);
        // ponemos que se ha creado
        _refUserCreated = true;

    }
    
    // Update is called once per frame
    void Update()
    {
    }


    public IEnumerator GetRecord(Action<int> callback) {
        while(!_refUserCreated){
            yield return null;
        }
        var task = _refUser.Child("Record").GetValueAsync();
            {
                int record = 0;
                yield return new WaitUntil(() => task.IsCompleted);
                if (task.IsFaulted)
                {
                    Debug.LogError("Error");
                }
                else if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result; 
                    record = int.Parse(snapshot.Value.ToString());
                }
                callback(record);
            }
    }

    public void UpdateScore(int scoreCount){
        _refUser.Child("Puntos").SetValueAsync(scoreCount);
    }

    public void UpdateRecord(int scoreCount){
        _refUser.Child("Record").SetValueAsync(scoreCount);
    }
}