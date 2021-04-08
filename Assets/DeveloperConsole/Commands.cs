using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Console
{
    public class Commands
    {
        #region variables
        private static Commands instance = null;

        private List<Command> _commands = new List<Command>();

        private List<Command> _commandsSingle;

        public static Commands Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Commands();
                }
                return instance;
            }
        }        
        #endregion

        #region Initialization

        public Commands()
        {
            RegisterCommands();
        }

    
        public List<Command> GetCommands()
        {
           return _commands.ToList();
        }
        public List<Command> GetCommandsSingle()
        {
            if (_commandsSingle == null)
            {
                List<Command> commandsSingle = new List<Command>();
                foreach (Command command in _commands)
                {
                    if (commandsSingle.Find(x=> x.GetQueryIdentity() == command.GetQueryIdentity()) == null)
                    {
                        commandsSingle.Add(command);
                    }
                }
                _commandsSingle = commandsSingle;
                return _commandsSingle.ToList();
            }
            return _commandsSingle.ToList();
        }

        public void RegisterCommands()
        {
            var commands = Utility.GetTypesWithCommandAttribute(System.AppDomain.CurrentDomain.GetAssemblies());

            foreach (Command command in commands)
            {
                if (_commands.Contains(command))//Check multiple stocking with instance
                {
                    DeveloperConsole.WriteWarning("Multiple stocking of command '" + command.GetQueryIdentity() + "'. Command will be ignored.");
                    continue;
                }
               
                var fields = command.GetType().GetFields();//Set command options
                foreach (FieldInfo fieldInfo in fields)
                {
                    if (fieldInfo.GetCustomAttribute<CommandParameterAttribute>() != null)
                    {
                        var commandParameterType = typeof(CommandParameter<>);
                        var commandParameterTypeGeneric = commandParameterType.MakeGenericType(fieldInfo.FieldType);
                        var commandParameter = Activator.CreateInstance(commandParameterTypeGeneric, new object[] { command, fieldInfo });
                        var commandParameterAttribute = fieldInfo.GetCustomAttribute<CommandParameterAttribute>();
                        command.commandParameters.Add(commandParameterAttribute.description, (CommandParameter)commandParameter);
                        commandParameterAttribute.commandParameter = (CommandParameter)commandParameter;
                    }
                }
                _commands.Add(command);
                //Debug.Log(command);
            }
            
            foreach (Command command in _commands.ToList())
            {
                if (String.IsNullOrEmpty(((ConsoleCommandAttribute)Attribute.GetCustomAttribute(command.GetType(), typeof(ConsoleCommandAttribute))).queryIdentity))
                {
                    var message = "Command " + command + "("+ command.GetHashCode()+") doesn't has a query identity. Command will be ignored." ;
                    Console.DeveloperConsole.WriteWarning(message);
                    _commands.Remove(command);
                }

                if (((ConsoleCommandAttribute)Attribute.GetCustomAttribute(command.GetType(), typeof(ConsoleCommandAttribute))).onlyAllowedOnDeveloperVersion && !Debug.isDebugBuild)
                {
                    _commands.Remove(command);
                }

                if (_commands.ToList().Exists(x => x.GetQueryIdentity() == command.GetQueryIdentity() && x != command))//Check multiple stocking with query identity
                {
                    var stockingCommands = _commands.ToList().FindAll(x => x.GetQueryIdentity() == command.GetQueryIdentity());//Get overstocking commands
                    List<Type> commandParamTypes = new List<Type>();

                    foreach (CommandParameter value in command.commandParameters.Values)
                    {
                        commandParamTypes.Add(value.genericType);
                    }

                    foreach (Command overStockedCommand in stockingCommands)//Check does overstocked commands have the same invoke definition
                    {
                        if (overStockedCommand == command)
                        {
                            continue;
                        }
                        List<Type> _paramTypes = new List<Type>();
                        foreach (CommandParameter value in overStockedCommand.commandParameters.Values)
                        {
                            _paramTypes.Add(value.genericType);
                        }
                        if (Utility.CompareLists<Type>(commandParamTypes, _paramTypes))
                        {
                            DeveloperConsole.WriteWarning("Conflict between two invoke definitions of command'" + command.GetQueryIdentity() + "'. Command will be ignored.");
                            _commands.Remove(command);

                            continue;
                        }
                    }
                }
            }            
        }
        #endregion

        #region commands
        [ConsoleCommand("help", "List all available commands.")]
        class Help : Command
        {
            public Help()
            {

            }

            public override ConsoleOutput Logic()
            {
                string commandList = " ";
                var commands = Commands.Instance.GetCommandsSingle().OrderBy(x => x.GetQueryIdentity()).ToList();

                foreach (Command command in commands)
                {
                    int lineLength = 0;

                    var line = "\n -" + command.GetQueryIdentity().ToLower();
                    lineLength = command.GetQueryIdentity().Length + 1;

                    var keys = command.commandParameters.Keys.ToArray();
                
                    for (int i = 0; i < keys.Length; i++)//Add description information to the line
                    {
                        var descriptionInfoString = " [" + keys[i].ToString() + "] ";
                        line += descriptionInfoString;
                        lineLength += descriptionInfoString.Length;
                    }

                    for (int i = 30 - lineLength; i > 0; i--)
                    {
                        line += " ";//Set orientation of command description 
                    }

                    line += command.GetDescription();

                    commandList += line;
                }
                return new ConsoleOutput("Available commands are " + commandList, ConsoleOutput.OutputType.System);
            }

        }

        [ConsoleCommand("help", "Provide help information for commands.")]
        class HelpCommand : Command
        {
            [CommandParameter("command")]
            public string queryIdentity;
            public HelpCommand()
            {

            }
            public override ConsoleOutput Logic()
            {
                var commands = Commands.Instance.GetCommands().FindAll(x => x.GetQueryIdentity() == queryIdentity);

                if (commands.Count == 0)
                {
                    return new ConsoleOutput("'"+queryIdentity + "' is not supported by help utility.", ConsoleOutput.OutputType.System);
                }

                string helpInformationText = "";

                int lineLength;
                foreach (Command command in commands)
                {

                    var line = "-" + command.GetQueryIdentity().ToLower();
                    lineLength = command.GetQueryIdentity().Length + 1;

                    var keys = command.commandParameters.Keys.ToArray();

                    for (int i = 0; i < keys.Length; i++)//Add description information to the line
                    {
                        var descriptionInfoString = " [" + keys[i].ToString() + "] ";
                        line += descriptionInfoString;
                        lineLength += descriptionInfoString.Length;
                    }

                    for (int i = 40 - lineLength; i > 0; i--)
                    {
                        line += " ";//Set orientation of command description 
                    }

                    line += command.GetDescription() + "\n";

                    helpInformationText += line;
                }
                return new ConsoleOutput(helpInformationText, ConsoleOutput.OutputType.System,false);
            }
        }

        [ConsoleCommand("move", "Translate a game object's transform to a world point.")]
        class Move : Command
        {
            [CommandParameter("Transform")]
            public Transform transform;
            [CommandParameter("Position")]
            public Vector3 position;

            public Move()
            {

            }

            public Move(Transform nameObj, Vector3 newPosition)
            {
                nameObj.position = newPosition;
            }

            public override ConsoleOutput Logic()
            {
                Move move = new Move(transform, position);

                //transform.position = position;
                
                return new ConsoleOutput(((Transform)transform).name + " moved to " + position.ToString() , ConsoleOutput.OutputType.Log);
            }
        }

        //test command add rigidbody
        [ConsoleCommand("add_rigidbody", "Add to object a component rigidbody.")]
        class AddRigidbody : Command
        {
            [CommandParameter("gameObject")]
            public Transform transform;

            public override ConsoleOutput Logic()
            {
                if (!transform.gameObject.GetComponent<Rigidbody>())
                {
                    transform.gameObject.AddComponent<Rigidbody>();

                    return new ConsoleOutput(transform.name + " add component Rigidbody", ConsoleOutput.OutputType.Log);
                }
                else if (transform.gameObject.GetComponent<Rigidbody>())
                {
                    return new ConsoleOutput("This object has this component!", ConsoleOutput.OutputType.Log);
                }
                else
                {
                    return new ConsoleOutput("Not valid object", ConsoleOutput.OutputType.Log);
                }
            }
        }

        //test list all objects in scene and in folder Resources
        [ConsoleCommand("all_obj", "Show all objects in hierarchy.")]
        class ShowListObjects : Command
        {
            public Transform[] trans = Transform.FindObjectsOfType<Transform>();

            public GameObject addresPref = GameObject.Find("AddressablePrefabs");

            string namesChild;
            string namesOb;

            LoadedAddressableLocation loaded = new LoadedAddressableLocation();

            public override ConsoleOutput Logic()
            {
                var addresPrefChilds = addresPref.GetComponentsInChildren<Transform>();

                loaded.InitResources();
                loaded.InitCommand();

                foreach (var nameObj in trans)
                {
                    string namesAddres = " ";
                    string namesAddressable;
                    for (int i = 0; i < addresPrefChilds.Length - 1; i++)
                    {
                        namesAddressable = addresPrefChilds[i + 1].name;
                        namesAddres += namesAddressable + ", ";
                    }

                    string commandString = "-";

                    var childCount = nameObj.childCount;

                    var child = nameObj.GetComponentsInChildren<Transform>();

                    var smt = nameObj.transform.parent;  //тут хранится объекты у которых есть дочерные объекты

                    if (childCount == 0 && smt == null)  //&&проверку не является ли объект уже дочерним
                    {
                        for (int i = 0; i < trans.Length; i++)
                        {
                            namesOb = trans[i].name;

                            commandString += namesOb + ", ";
                        }
                        Debug.Log("In Scene:" + commandString + namesAddres);
                    }

                    if (childCount != 0)
                    {
                        for (int i = 0; i < child.Length - 1; i++)
                        {
                            namesChild = child[i + 1].name;

                            //Debug.Log(namesChild);  //выводит все имена дочерних объектов

                            commandString += namesChild + ", ";
                        }

                        //Debug.Log(nameObj.name + " with childs:" + commandString);  //вывожу имена родителя с дочерними объектами в одну строку
                        //Debug.Log(nameObj.name);  //выводит имена объектов у которых есть дочерние объекты
                        //Debug.Log(commandString);  //выводит имена дочерних объектов в одну строку
                    }
                }

                return new ConsoleOutput("Number of objects: " + trans.Length, ConsoleOutput.OutputType.Log);
            }
        }
        
        //test anim bool command
        [ConsoleCommand("anim_bool", "Play some animation.")]
        class AnimBool : Command
        {
            [CommandParameter("gameObject")]
            public Transform gameObject;
            [CommandParameter("nameParameters")]
            public string nameParameters;
            [CommandParameter("parameters")]
            public bool parameters;

            public override ConsoleOutput Logic()
            {
                var anim = gameObject.GetComponent<Animator>();

                if (anim != null)
                {
                    anim.SetBool(nameParameters, parameters);
                }
                else return new ConsoleOutput("Don't have anim controller!", ConsoleOutput.OutputType.Log);

                return new ConsoleOutput(string.Empty, ConsoleOutput.OutputType.Log);
            }

        }

        //test anim trigger command
        [ConsoleCommand("anim_trigger", "Play some animation.")]
        class AnimTrigger : Command
        {
            [CommandParameter("gameObject")]
            public Transform gameObject;
            [CommandParameter("nameParameters")]
            public string nameParameters;

            public override ConsoleOutput Logic()
            {
                var anim = gameObject.GetComponent<Animator>();
                var colid = gameObject.GetComponent<CapsuleCollider>();
                var peep = gameObject.transform.position;

                if (anim != null)
                {
                    colid.direction = 2;  //меняю  направления коллайдера: 0=х, 1=у, 2=z
                    anim.SetTrigger(nameParameters);
                    peep.z = -10.1f;
                }
                else return new ConsoleOutput("Don't have anim controller!", ConsoleOutput.OutputType.Log);

                return new ConsoleOutput(string.Empty, ConsoleOutput.OutputType.Log);
            }

        }

        //test instantiate object
        [ConsoleCommand("create_ob", "Create object from assets.")]
        class CreateObject : Command
        {
            [CommandParameter("Object")]
            public AssetReference gameObject;

            private GameObject instansiatedObject;
            public GameObject addresPref = GameObject.Find("AddressablePrefabs");

            public GameObject InstansiatedObject { get => instansiatedObject; set => instansiatedObject = value; }

            public void ObjectLoadDone(AsyncOperationHandle<GameObject> obj)
            {
                if (obj.Status == AsyncOperationStatus.Succeeded)
                {
                    var loadedObject = obj.Result;

                    Vector3 mousePosition = Input.mousePosition;
                    RaycastHit hit;
                    Ray ray = Camera.main.ScreenPointToRay(mousePosition);
                    if (Physics.Raycast(ray, out hit))
                    {
                        InstansiatedObject = GameObject.Instantiate(loadedObject, hit.point, Quaternion.identity);
                        instansiatedObject.transform.SetParent(addresPref.transform);  //помещаю созданый объект в пустышку на сцене для контроля
                        //Debug.Log("Position " + hit.point.ToString());

                        InstansiatedObject.name = obj.Result.name;  //устанавливаю новое имя клону объекта
                    }
                }
            }

            public override ConsoleOutput Logic()
            {
                Addressables.LoadAssetAsync<GameObject>(gameObject).Completed += ObjectLoadDone;

                DeveloperConsole.Instance.input = "move";

                return new ConsoleOutput(string.Empty, ConsoleOutput.OutputType.Log);
            }
        }

        [ConsoleCommand("destroy", "Destroy a game object")]
        class DestroyObj : Command
        {
            [CommandParameter("nameObj")]
            public string nameObj;

            public override ConsoleOutput Logic()
            {
                var smt = GameObject.FindObjectOfType<GameObject>();

                if (smt.name == nameObj)
                {
                    UnityEngine.Object.Destroy(smt);
                }
                return new ConsoleOutput(nameObj + " was delete.", ConsoleOutput.OutputType.Log);
            }
        }

        //test
        [ConsoleCommand("move_rotate", "Create object and move him to point")]
        class CreateAndMove : Command
        {
            [CommandParameter("Object")]
            public Transform gameObject;

            [CommandParameter("Position")]
            public Vector3 position;

            [CommandParameter("Rotation")]
            public Quaternion quaternion;

            public Queue<CommandParameter> parameters;  //список команд

            public override ConsoleOutput Logic()
            {
                Move move = new Move(gameObject, position);

                gameObject.rotation = quaternion;

                return new ConsoleOutput("Done bich!", ConsoleOutput.OutputType.Log);
            }
        }

        //test
        [ConsoleCommand("get_param", "Get command parameters")]
        public class GetParam : Command
        {
            [CommandParameter("GameObject")]
            public Transform gameObject;

            [CommandParameter("Position")]
            public Vector3 position;

            public Queue<object> parameters = new Queue<object>();

            void GetListParam()
            {
                var commands = Commands.Instance.GetCommandsSingle().OrderBy(x => x.GetQueryIdentity()).ToList();  //тут хранятся все команды

                foreach (var item in commands)
                {
                    //Debug.Log(item);  //получаю список всех консольных команд

                    //Debug.Log(item.commandParameters.Count);  //сколкьо параметров принипмаект команда

                    //parameters.Enqueue(item);

                    //Debug.Log(item.GetQueryIdentity());

                    //if (item.GetQueryIdentity() == "move")
                    //{
                    //    parameters.Enqueue(item);
                    //    list.Add(item);
                    //}
                    //if (item.GetQueryIdentity() == "create_ob")
                    //{
                    //    parameters.Enqueue(item);
                    //    list.Add(item);
                    //    Debug.Log("param name is " + item);
                    //}
                    //if (item.GetQueryIdentity() == "destroy")
                    //{
                    //    parameters.Enqueue(item);
                    //    list.Add(item);
                    //}

                }

                //var commandParameterType = typeof(CommandParameter<>);
                //var commandParameterTypeGeneric = commandParameterType.MakeGenericType(fieldInfo.FieldType);
                //var commandParameter = Activator.CreateInstance(commandParameterTypeGeneric, new object[] { command, fieldInfo });
                //var commandParameterAttribute = fieldInfo.GetCustomAttribute<CommandParameterAttribute>();
                //command.commandParameters.Add(commandParameterAttribute.description, (CommandParameter)commandParameter);
                //commandParameterAttribute.commandParameter = (CommandParameter)commandParameter;

                foreach (var item in parameters)
                {
                    Debug.Log("list param" + item.ToString());
                }
            }

            public override ConsoleOutput Logic()
            {
                parameters.Enqueue(gameObject);
                parameters.Enqueue(position);

                GetListParam();

                //Move move = new Move(gameObject, position);

                Debug.Log(parameters.Count + " count param");

                return base.Logic();
            }
        }

        [ConsoleCommand("rotate", "Rotate a game object.")]
        class Rotate : Command
        {
            [CommandParameter("transform")]
            public Transform transform;
            [CommandParameter("rotation")]
            public Quaternion rotation;
            public Rotate()
            {

            }

            public override ConsoleOutput Logic()
            {
                transform.rotation = rotation;
                return new ConsoleOutput(((Transform)transform).name + " rotated to " + rotation.ToString(), ConsoleOutput.OutputType.Log);
            }
        }

        [ConsoleCommand("sphere", "Instantiate a physical sphere.")]
        class Sphere : Command
        {
            public Sphere()
            {

            }

            public override ConsoleOutput Logic()
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                var rigidbody = sphere.AddComponent<Rigidbody>();

                RaycastHit hit;
                Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward); ;
                if (Physics.Raycast(ray,out hit))
                {
                    sphere.transform.position = hit.point;
                }
                else
                {
                    sphere.transform.position = Camera.main.transform.position + Camera.main.transform.forward *5f;
                }

                return new ConsoleOutput("Sphere created at " + sphere.transform.position.ToString(), ConsoleOutput.OutputType.Log);
            }

        }

        [ConsoleCommand("export","Export this session to a text file.")]
        class Export : Command
        {

            public override ConsoleOutput Logic()
            {
                base.Logic();

                var outputs = DeveloperConsole.Instance.consoleOutputs;
                var src = DateTime.Now;

                string fileName = "console-"+src.Year + "-" + src.Hour + "-" + src.Minute+".txt";
                string fileContent = "";

                foreach (ConsoleOutput consoleOutput in outputs)
                {
                    fileContent += consoleOutput.output + "\n";
                }
                string filePath = Directory.GetParent(Application.dataPath)+"/Logs/" + fileName;
                var output = File.CreateText(filePath);

                output.Write(fileContent);

                output.Close();

                return new ConsoleOutput("Log file created at '" + filePath + "'", ConsoleOutput.OutputType.Log);
            }
        }

        [ConsoleCommand("beep", "Play the sound associated with the beep system event.")]
        class Beep : Command
        {

            public override ConsoleOutput Logic()
            {
                base.Logic();
                //System.Media.SystemSounds.Beep.Play();
                return new ConsoleOutput("Beeping", ConsoleOutput.OutputType.Log);
            }
        }

        [ConsoleCommand("quit", "Exit the application")]
        class Quit : Command
        {

            public override ConsoleOutput Logic()
            {
                base.Logic();
                Application.Quit();
                return new ConsoleOutput("Have a very safe and productive day.", ConsoleOutput.OutputType.Log);
            }
        }

        [ConsoleCommand("echo", "Echo text to console.")]
        class Echo : Command
        {
            [CommandParameter("string")]
            public string echoText;
            public override ConsoleOutput Logic()
            {
                base.Logic();
                
                return new ConsoleOutput(echoText, ConsoleOutput.OutputType.Log,false);
            }
        }

        [ConsoleCommand("fps_max", "Limit the frame rate. Set 0 for unlimited.")]
        class Fps_max : Command
        {
            [CommandParameter("maxFPS")]
            public int maxFPS;
            public override ConsoleOutput Logic()
            {
                base.Logic();
                Application.targetFrameRate = maxFPS;
                return new ConsoleOutput("Frame rate limited to " + maxFPS+" frames per second", ConsoleOutput.OutputType.Log);
            }
        }

        [ConsoleCommand("screenshot", "Save a screenshot.")]
        class Screenshot : Command
        {
            
            public override ConsoleOutput Logic()
            {
                base.Logic();
                var src = DateTime.Now;
                string fileName = "screenshot-" + src.Year + "-" + src.Hour + "-" + src.Minute + ".png";
                string filePath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) +"/"+fileName;
                ScreenCapture.CaptureScreenshot(filePath);
                return new ConsoleOutput("Screenshot saved to " + filePath + ".", ConsoleOutput.OutputType.Log);
            }
        }

        [ConsoleCommand("loadlevel", "Load the level by given name or id.")]
        class LoadLevel : Command
        {
            [CommandParameter("level")]
            public string targetLevel;

            public override ConsoleOutput Logic()
            {
                base.Logic();
                var levelId = 0;
                if (int.TryParse(targetLevel, out levelId))
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(levelId);
                    return new ConsoleOutput("Loading level with id " + levelId + ".", ConsoleOutput.OutputType.Log);
                }
                
                UnityEngine.SceneManagement.SceneManager.LoadScene(targetLevel);
                return new ConsoleOutput("Loading level " + targetLevel + ".", ConsoleOutput.OutputType.Log);

            }
        }

        [ConsoleCommand("ping", "Ping adress.")]
        class ping : Command
        {
            [CommandParameter("adress")]
            public string targetLevel;

            public override ConsoleOutput Logic()
            {
                base.Logic();
                System.Net.NetworkInformation.Ping pingSender = new System.Net.NetworkInformation.Ping();

                // Wait 10 seconds for a reply.
                int timeout = 1000;

                byte[] buffer = System.Text.Encoding.ASCII.GetBytes(targetLevel);
                System.Net.NetworkInformation.PingOptions options = new System.Net.NetworkInformation.PingOptions(64, true);
                System.Net.NetworkInformation.PingReply reply = null;
                try
                {
                    reply = pingSender.Send(targetLevel, timeout, buffer, options);

                }
                catch (Exception ex)
                {
                    return new ConsoleOutput("Transmit faild. "+ ex.Message , ConsoleOutput.OutputType.Error);

                }

                return new ConsoleOutput("Reply from " + reply.Address + ": bytes=" + reply.Buffer.Length+" time=" + reply.RoundtripTime+"ms Status=" + reply.Status, ConsoleOutput.OutputType.Network);

            }
        }
        
        [ConsoleCommand("fov", "Set field of view of the current camera.")]
        class FieldOfView : Command
        {
            [CommandParameter("float")]
            public float value;

            public override ConsoleOutput Logic()
            {
                base.Logic();
                Camera.main.fieldOfView = value;

                return new ConsoleOutput("", ConsoleOutput.OutputType.Log);

            }
        }

        [ConsoleCommand("clear", "Clear all console output")]
        class ClearField : Command
        {
            public override ConsoleOutput Logic()
            {
                base.Logic();
                DeveloperConsole.Instance.consoleOutputs.Clear();
                return new ConsoleOutput("A new start.", ConsoleOutput.OutputType.Log);

            }
        }

        [ConsoleCommand("time_scale", "Scale time.")]
        class Time_Scale : Command
        {
            [CommandParameter("float")]
            public float value;

            public override ConsoleOutput Logic()
            {
                base.Logic();
                Time.timeScale = value;
                return new ConsoleOutput("", ConsoleOutput.OutputType.Log, false);

            }
        }

        [ConsoleCommand("time_fixeddeltatime", "")]
        class Time_Fixedtimestep : Command
        {
            [CommandParameter("float")]
            public float value;

            public override ConsoleOutput Logic()
            {
                base.Logic();
                Time.fixedDeltaTime = value;
                return new ConsoleOutput("", ConsoleOutput.OutputType.Log, false);

            }
        }

        [ConsoleCommand("hourglass", "Print the time since startup.")]
        class Hourglass : Command
        {
            public override ConsoleOutput Logic()
            {
                base.Logic();
                return new ConsoleOutput("Engine is running for "+(int)Time.realtimeSinceStartup + " seconds." , ConsoleOutput.OutputType.Log);

            }
        }

        [ConsoleCommand("flush", "Clear cache memory.")]
        class Flush : Command
        {
            public override ConsoleOutput Logic()
            {
                base.Logic();
                var cacheCount = Caching.cacheCount;
                Caching.ClearCache();
                return new ConsoleOutput("Cleared " + cacheCount + " cache(s).", ConsoleOutput.OutputType.Log);

            }
        }

        [ConsoleCommand("fog_color", "")]
        class Fog_Color : Command
        {

            [CommandParameter("r,g,b")]
            public Color Color;
            public override ConsoleOutput Logic()
            {
                base.Logic();
                RenderSettings.fog = true;

                RenderSettings.fogColor = Color;

                return new ConsoleOutput("", ConsoleOutput.OutputType.Log,false);

            }
        }

        [ConsoleCommand("fog_active", "")]
        class Fog_Active : Command
        {

            [CommandParameter("bool")]
            public bool fog;
            public override ConsoleOutput Logic()
            {
                base.Logic();
                RenderSettings.fog = fog;


                return new ConsoleOutput("", ConsoleOutput.OutputType.Log, false);

            }
        }

        [ConsoleCommand("fog_start", "")]
        class Fog_Start : Command
        {

            [CommandParameter("float")]
            public float start;
            public override ConsoleOutput Logic()
            {
                base.Logic();
                RenderSettings.fogStartDistance = start;


                return new ConsoleOutput("", ConsoleOutput.OutputType.Log, false);

            }
        }

        [ConsoleCommand("fog_end", "")]
        class Fog_End : Command
        {

            [CommandParameter("float")]
            public float start;
            public override ConsoleOutput Logic()
            {
                base.Logic();
                RenderSettings.fogEndDistance = start;


                return new ConsoleOutput("", ConsoleOutput.OutputType.Log, false);

            }
        }

        [ConsoleCommand("allocmem", "Print the amount of allocated memory for graphics driver.")]
        class Drawcalls : Command
        {

        
            public override ConsoleOutput Logic()
            {
                base.Logic();
                var bytes = UnityEngine.Profiling.Profiler.GetAllocatedMemoryForGraphicsDriver();


                return new ConsoleOutput(bytes+" bytes", ConsoleOutput.OutputType.Log, false);

            }
        }

        [ConsoleCommand("path", "Print the engine filesystem path.")]
        class GetPath : Command
        {

            public override ConsoleOutput Logic()
            {
                base.Logic();
                var path = Directory.GetParent(Application.dataPath);


                return new ConsoleOutput(path.ToString(), ConsoleOutput.OutputType.Log, false);

            }
        }

        [ConsoleCommand("phys_gravity", "")]
        class Phys_Gravity : Command
        {
            [CommandParameter("Vector3")]
            public Vector3 gravity;
            public override ConsoleOutput Logic()
            {
                base.Logic();
                Physics.gravity = gravity;

                return new ConsoleOutput("", ConsoleOutput.OutputType.Log, false);

            }
        }
        [ConsoleCommand("phys_bouncethreshold", "")]
        class Phys_Bouncethreshold : Command
        {
            [CommandParameter("float")]
            public float value;
            public override ConsoleOutput Logic()
            {
                base.Logic();
                Physics.bounceThreshold = value;

                return new ConsoleOutput("", ConsoleOutput.OutputType.Log, false);

            }
        }
        [ConsoleCommand("phys_sleepthreshold", "")]
        class Phys_Sleepthreshold : Command
        {
            [CommandParameter("float")]
            public float value;
            public override ConsoleOutput Logic()
            {
                base.Logic();
                Physics.sleepThreshold = value;

                return new ConsoleOutput("", ConsoleOutput.OutputType.Log, false);

            }
        }
        [ConsoleCommand("phys_contactoffset", "Set the contact offset of the newly created colliders.")]
        class Phys_Sleepvelocity : Command
        {
            [CommandParameter("float")]
            public float value;
            public override ConsoleOutput Logic()
            {
                base.Logic();
                Physics.defaultContactOffset = value;

                return new ConsoleOutput("", ConsoleOutput.OutputType.Log, false);

            }
        }
        [ConsoleCommand("phys_maxangular", "Set the maximum angular speed.")]
        class Phys_Maxangular : Command
        {
            [CommandParameter("float")]
            public float value;
            public override ConsoleOutput Logic()
            {
                base.Logic();
                Physics.defaultMaxAngularSpeed = value;

                return new ConsoleOutput("", ConsoleOutput.OutputType.Log, false);

            }
        }

        [ConsoleCommand("phys_clothgravity", "")]
        class Phys_Clothgravity : Command
        {
            [CommandParameter("Vector3")]
            public Vector3 value;
            public override ConsoleOutput Logic()
            {
                base.Logic();
                Physics.clothGravity = value;

                return new ConsoleOutput("", ConsoleOutput.OutputType.Log, false);

            }
        }

        [ConsoleCommand("ren_shadows", "Determine which type of shadows should be used. 0-Disable 1-Hard Only 2-All")]
        class Ren_Shadowquality : Command
        {
            [CommandParameter("ShadowQuality")]
            public int value;
            public override ConsoleOutput Logic()
            {
                base.Logic();
                QualitySettings.shadows = (ShadowQuality)value;

                return new ConsoleOutput("", ConsoleOutput.OutputType.Log, false);

            }
        }

        [ConsoleCommand("ren_shadowresolution", "0-Low 1-Medium 2-High 3-Vey High")]
        class Ren_Shadowresolution : Command
        {
            [CommandParameter("resolution")]
            public int value;
            public override ConsoleOutput Logic()
            {
                base.Logic();
                QualitySettings.shadowResolution = (ShadowResolution)value;

                return new ConsoleOutput("", ConsoleOutput.OutputType.Log, false);

            }
        }

        [ConsoleCommand("ren_softParticles", "")]
        class Ren_Softparticles : Command
        {
            [CommandParameter("bool")]
            public bool value;
            public override ConsoleOutput Logic()
            {
                base.Logic();
                QualitySettings.softParticles = value;

                return new ConsoleOutput("", ConsoleOutput.OutputType.Log, false);

            }
        }

        [ConsoleCommand("ren_antialiasing", "")]
        class Ren_Antialiasing : Command
        {
            [CommandParameter("int")]
            public int value;
            public override ConsoleOutput Logic()
            {
                base.Logic();
                QualitySettings.antiAliasing = value;

                return new ConsoleOutput("", ConsoleOutput.OutputType.Log, false);

            }
        }

        [ConsoleCommand("ren_staringbillb", "Face billboards towards the camera.")]
        class Ren_Staringbilb : Command
        {
            [CommandParameter("bool")]
            public bool value;
            public override ConsoleOutput Logic()
            {
                base.Logic();
                QualitySettings.billboardsFaceCameraPosition = value;

                return new ConsoleOutput("", ConsoleOutput.OutputType.Log, false);

            }
        }
        [ConsoleCommand("ren_increase", "Increase the current quality level.")]
        class Ren_Increase : Command
        {
            public override ConsoleOutput Logic()
            {
                base.Logic();
                QualitySettings.IncreaseLevel();

                return new ConsoleOutput("", ConsoleOutput.OutputType.Log, false);

            }
        }
        [ConsoleCommand("ren_decrease", "Increase the current quality level.")]
        class Ren_Decrease : Command
        {
            public override ConsoleOutput Logic()
            {
                base.Logic();
                QualitySettings.DecreaseLevel();

                return new ConsoleOutput("", ConsoleOutput.OutputType.Log, false);

            }
        }
        [ConsoleCommand("ren_lodbias", "Set the global multiplier for the LOD's switching distance.")]
        class Ren_Lodbias : Command
        {
            [CommandParameter("float")]
            public float value;
            public override ConsoleOutput Logic()
            {
                base.Logic();
                QualitySettings.lodBias = value;

                return new ConsoleOutput("", ConsoleOutput.OutputType.Log, false);

            }
        }
        [ConsoleCommand("ren_pxllightcount", "Set the maximum number of pixel lights that should affect any object.")]
        class Ren_Pixellightcount : Command
        {
            [CommandParameter("int")]
            public int value;
            public override ConsoleOutput Logic()
            {
                base.Logic();
                QualitySettings.pixelLightCount = value;

                return new ConsoleOutput("", ConsoleOutput.OutputType.Log, false);

            }
        }
        [ConsoleCommand("ren_rltrefprobes", "Enable realtime reflection probes.")]
        class Ren_Realtimereflectionprobes : Command
        {
            [CommandParameter("bool")]
            public bool value;
            public override ConsoleOutput Logic()
            {
                base.Logic();
                QualitySettings.realtimeReflectionProbes = value;

                return new ConsoleOutput("", ConsoleOutput.OutputType.Log, false);

            }
        }
        [ConsoleCommand("ren_vsynccount", "The VSync Count.")]
        class Ren_VSynchcount : Command
        {
            [CommandParameter("int")]
            public int value;
            public override ConsoleOutput Logic()
            {
                base.Logic();
                QualitySettings.vSyncCount = value;

                return new ConsoleOutput("", ConsoleOutput.OutputType.Log, false);

            }
        }

        [ConsoleCommand("restart", "Restart the game on the same level.")]
        class Restart : Command
        {
 
            public override ConsoleOutput Logic()
            {
                base.Logic();
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

                return new ConsoleOutput("", ConsoleOutput.OutputType.Log, false);

            }
        }

        [ConsoleCommand("au_volume", "Set volume of the current audio listener.")]
        class Au_Volume : Command
        {
            [CommandParameter("float")]
            public float value;
            public override ConsoleOutput Logic()
            {
                base.Logic();
                AudioListener.volume = value;

                return new ConsoleOutput("", ConsoleOutput.OutputType.Log, false);

            }
        }

        [ConsoleCommand("net_clusterstat", "Print the status of cluster network.")]
        class Net_ClusterStatus : Command
        {
            public override ConsoleOutput Logic()
            {
                base.Logic();               

                return new ConsoleOutput("Cluster network status: isDisconnected="+ ClusterNetwork.isDisconnected+ " isMasterOfCluster="+ ClusterNetwork.isMasterOfCluster+ " nodeIndex="+ClusterNetwork.nodeIndex, ConsoleOutput.OutputType.Network, false);
            }
        }

        [ConsoleCommand("ren_wireframe", "Enable wireframe mode.",true)]
        class Ren_wireframe : Command
        {
            [CommandParameter("bool")]
            public bool value;
            public override ConsoleOutput Logic()
            {
                base.Logic();

                if (value)
                {
                    if (Camera.main.GetComponent<WireframeWidget>() == null)
                    {
                        Camera.main.gameObject.AddComponent<WireframeWidget>();

                    }
                    Camera.main.GetComponent<WireframeWidget>().enabled = true;
                    return new ConsoleOutput("Wireframe mode enabled", ConsoleOutput.OutputType.Log, false);

                }

                if (Camera.main.GetComponent<WireframeWidget>() != null)
                {
                    Camera.main.gameObject.GetComponent<WireframeWidget>().enabled = false;
                    return new ConsoleOutput("Wireframe mode disabled", ConsoleOutput.OutputType.Log, false);

                }

                return new ConsoleOutput("", ConsoleOutput.OutputType.Log, false);
            }
        }

        [ConsoleCommand("culture", "Set the culture.")]
        class CultureSet : Command
        {
            [CommandParameter("CultureInfo")]
            public System.Globalization.CultureInfo value;
            public override ConsoleOutput Logic()
            {
                base.Logic();
                var cultureInfo = value;
                System.Globalization.CultureInfo.CurrentCulture = value;

                return new ConsoleOutput("Culture is now "+ cultureInfo, ConsoleOutput.OutputType.Log, false);

            }
        }
        [ConsoleCommand("culture", "Get the culture.")]
        class CultureGet : Command
        {

            public override ConsoleOutput Logic()
            {
                base.Logic();
                var oldCulture = System.Globalization.CultureInfo.CurrentCulture.Name;

                return new ConsoleOutput("Culture is " + oldCulture, ConsoleOutput.OutputType.Log, false);

            }

        }

        [ConsoleCommand("rb_addforce", "Addforce to a rigidbody.")]
        class Addforce : Command
        {
            [CommandParameter("Rigidbody")]
            public Rigidbody Rigidbody;

            [CommandParameter("Vector3")]
            public Vector3 force;
            public override ConsoleOutput Logic()
            {
                base.Logic();

                Rigidbody.AddForce(force);

                return new ConsoleOutput("Force " + force.ToString()+ " applied to object "+ Rigidbody.name, ConsoleOutput.OutputType.Log, true);

            }
        }

        [ConsoleCommand("rb_mass", "Set mass of rigidbody.")]
        class SetMass : Command
        {
            [CommandParameter("Rigidbody")]
            public Rigidbody Rigidbody;

            [CommandParameter("Vector3")]
            public float mass;
            public override ConsoleOutput Logic()
            {
                base.Logic();

                Rigidbody.mass = mass;

                return new ConsoleOutput("", ConsoleOutput.OutputType.Log, false);

            }
        }

        [ConsoleCommand("rb_drag", "Set drag of object.")]
        class SetDrag : Command
        {
            [CommandParameter("Rigidbody")]
            public Rigidbody Rigidbody;

            [CommandParameter("Vector3")]
            public float drag;
            public override ConsoleOutput Logic()
            {
                base.Logic();

                Rigidbody.drag = drag;

                return new ConsoleOutput("", ConsoleOutput.OutputType.Log, false);

            }
        }
        [ConsoleCommand("rb_freezerot", "Freeze rotation of object.")]
        class FreezeRotation : Command
        {
            [CommandParameter("Rigidbody")]
            public Rigidbody Rigidbody;
            [CommandParameter("bool")]
            public bool value;

 
            public override ConsoleOutput Logic()
            {
                base.Logic();

                Rigidbody.freezeRotation = value;

                return new ConsoleOutput("", ConsoleOutput.OutputType.Log, false);

            }
        }

        [ConsoleCommand("rb_usegravity", "Freeze position of object.")]
        class Usegravity : Command
        {
            [CommandParameter("Rigidbody")]
            public Rigidbody Rigidbody;
            [CommandParameter("bool")]
            public bool value;


            public override ConsoleOutput Logic()
            {
                base.Logic();
                
                    Rigidbody.useGravity = value;
                
                return new ConsoleOutput("", ConsoleOutput.OutputType.Log, false);

            }
        }
        #endregion
    }
}