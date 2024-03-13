 # FireBase Unity
 ## Crear la base de datos
 En este caso usaremos la base de datos de realtime, en el que crearemos dos ramas, una con los pickups que usaremos para crearlas en nuestro juegos y tambien crearemos otra rama con Jugadores:

Prefabs tendrá esta estructura pero creando todos los que queremaos

 - Prefabs
    - p1
        - x = ...
        - y = 0.5
        - z = ...
    - p2
        - x = ...
        - y = 0.5
        - z = ...

Mientras que jugadores tendra la siguiente estructura

- Jugadores
    - idJugador
        - Puntos = ...
        - Record = ...
        - nombre = "Mi dispositivo"
        - posicion = [0,0.5,0]

![imagen](imagenes/estructuraDB)

## Uso de la base de datos desde el script
Crearemos un objeto vacio al que le añadiremos un script que contenga las funciones que usen la base de datos:

Imports del script:
```c#
using System.Collections;
using System.Collections.Generic; // para las posiciones del jugador
using System;
using UnityEngine;

using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using Unity.VisualScripting;
using System.Threading.Tasks;
```
Variables del programa
```c#
// conexion con Firebase (_app(para decir que es privada))
private FirebaseApp _app;
// Singleton de la Base de Datos
private FirebaseDatabase _db;
```

Para inicializar la `FirebaseApp` cree una funcion que la retornara un FirebaseApp asi que asignare a _app a la funcion:
```c#
// realizamos la conexion a Firebase
// devolvemos una instancia de esta aplicacion
FirebaseApp Conexion() {
    FirebaseApp firebaseApp = null;
    FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
    {
        var dependencyStatus = task.Result;
        if (dependencyStatus == DependencyStatus.Available) {
            // Create and hold a reference to your FirebaseApp,
            // where app is a Firebase.FirebaseApp property of your application class.
            firebaseApp = FirebaseApp.DefaultInstance;
            // Set a flag here to indicate whether Firebase is ready to use by your app.
        }
        else {
            Debug.LogError(System.String.Format(
                "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
            // Firebase Unity SDK is not safe to use here.
            firebaseApp = null;
        }
    });
        
    return firebaseApp;
}
```

Y a la variable `FirebaseDatabase _db` le asignamos lo siguiente : `FirebaseDatabase.DefaultInstance`

### Añadir jugador
```csharp
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
```
En resumen, este bloque de código se encarga de crear un nodo para el jugador en la base de datos Firebase, asignarle datos iniciales y establecer su estado de conexión cuando el jugador inicia sesión en el juego.

1. **Obtencion del Identificador Único del Dispositivo**
```csharp
_userId = SystemInfo.deviceUniqueIdentifier;
```
Aquí se recoge el identificador único del dispositivo. Este identificador se utiliza para identificar de forma única al jugador en la base de datos.

2. **Comprobación de la Existencia del Jugador en la Base de Datos:**
```csharp
DataSnapshot snapshot = await _refClientes.Child(_userId).GetValueAsync();
if(!snapshot.Exists){
    // Si no existe, se crea el jugador con datos iniciales
}
```

3. **Creación del Jugador en la Base de Datos:**
```csharp
_refClientes.Child(_userId).Child("nombre").SetValueAsync("Mi dispositivo");
_refClientes.Child(_userId).Child("Puntos").SetValueAsync(0);
_refClientes.Child(_userId).Child("Record").SetValueAsync(0);
```
Si el jugador no existe en la base de datos, se creará un nuevo nodo con tres hijos: "nombre" (que se establece como "Mi dispositivo" en este caso), "Puntos" (inicializado en 0) y "Record" (también inicializado en 0).

4. **Añadir la Posición Inicial del Jugador:**
```csharp
Dictionary<string, object> posicion = new Dictionary<string, object>();
posicion["x"] = 0; // Valor inicial de x
posicion["y"] = 0; // Valor inicial de y
posicion["z"] = 0; // Valor inicial de z
_refClientes.Child(_userId).Child("posicion").SetValueAsync(posicion);
```
Se crea un diccionario con las coordenadas de la posición inicial del jugador (x, y, z) y se añade como un hijo del nodo del jugador en la base de datos.

5. **Establecer el Estado de Conexión del Jugador:**
```csharp
_refClientes.Child(_userId).Child("connected").SetValueAsync(true);
```
Se establece el estado de conexión del jugador como "true" para indicar que está conectado.

## Recoger el record
```csharp
public IEnumerator GetRecord(Action<int> callback) {
        // se espera a que se haya creado la referencia de usuario
        while(!_refUserCreated){
            yield return null;
        }
        // cogemos el valor record y lo devolvemos ya que lo recibe el jugador
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
```


## Añadir Prefabs a partir de la base de datos

```csharp
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
```

Este método, `AñadirPrefabs`, es responsable de tomar datos de un `DataSnapshot` y utilizarlos para instanciar prefabs en posiciones específicas en el espacio de juego.

### Funcionamiento

El método recorre cada elemento dentro del `DataSnapshot`, que contiene información sobre los prefabs y sus posiciones. Luego, para cada prefab, extrae las coordenadas de posición (`x`, `y` y `z`) y las utiliza para crear un objeto en esa posición en el espacio de juego.

### Uso

Para utilizar esta función, se debe proporcionar un `DataSnapshot` que contenga información estructurada de la siguiente manera:

- Cada elemento del `DataSnapshot` representa un prefab.
- Cada prefab debe contener las claves `"x"`, `"y"` y `"z"`, que representan las coordenadas de posición del prefab.
- Los valores asociados a estas claves deben ser números que representen las coordenadas respectivas.

## Ejemplo de Uso
```csharp
// Se llama a la función AñadirPrefabs con el DataSnapshot adecuado
AñadirPrefabs(snapshot);
```


### Notas Adicionales

- Asegúrate de que el objeto `pickUp` esté asignado correctamente antes de llamar a esta función.
- Asegúrate de que el `DataSnapshot` proporcionado esté estructurado según lo esperado por esta función.

## Modificación de la posición
```csharp
public void UpdatePosition(Dictionary<string, object> posicion){
    _refUser.Child("posicion").SetValueAsync(posicion);
}
```
Esta función se llama desde el player controller, recibiendo un diccionario con sus posiciones

## Poner el valor connected a false al salir del juego
```csharp
private void OnApplicationQuit() {
    _refUser.Child("connected").SetValueAsync(false);
}
```
El `OnApplicationQuit()` es un método que trae Unity por defecto