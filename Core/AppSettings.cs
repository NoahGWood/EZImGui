using YamlDotNet.Serialization;

namespace EZImGui.Core
{
    public interface ISettings
    {
        public void LoadSettings();
        public void SaveSettings();
        public object GetSettings();
    }
    public class AppSettings<T> : ISettings where T : class
    {
        public static string SettingsFilePath = "settings.yaml";
        
        public List<IYamlTypeConverter> TypeConverters = new();

        public delegate void HandleSettings(T settingsModel);
        public HandleSettings SettingsHandler;
        public T SettingsModel { get; set; }
        
        public AppSettings() { SettingsModel = Activator.CreateInstance<T>(); }
        public AppSettings(string fpath) { SettingsFilePath = fpath; }
        
        public void AddTypeConverter(IYamlTypeConverter type) { TypeConverters.Add(type); SettingsModel = Activator.CreateInstance<T>(); }

        public void LoadSettings()
        {
            if(File.Exists(SettingsFilePath))
            {
                var deserbuild = new DeserializerBuilder();
                foreach(IYamlTypeConverter typeConverter in TypeConverters)
                {
                    deserbuild.WithTypeConverter(typeConverter);
                }
                var deser = deserbuild.Build();
                var yaml = File.ReadAllText(SettingsFilePath);
                T settings = deser.Deserialize<T>(yaml);
                SettingsModel = settings;
                if(SettingsHandler != null)
                    SettingsHandler(settings);
                Logger.CoreDebug("Loaded Settings.");
            } else
            {
                Logger.CoreDebug("No Settings File Found.");
            }
        }
        public void SaveSettings(T settingsModel)
        {
            var serbuild = new SerializerBuilder();
            foreach(var typeConverter in TypeConverters)
            {
                serbuild.WithTypeConverter(typeConverter);
            }
            var serializer = serbuild.Build();
            var yaml = serializer.Serialize(settingsModel);
            File.WriteAllText(SettingsFilePath, yaml);
        }

        public void SaveSettings()
        {
            SaveSettings(SettingsModel);
        }

        public object GetSettings() { return SettingsModel;  }
    }

    /*
     * Example SettingsModel Class 
     * 
     * class SettingsModel
     * {
     *     public string ApiKey { get; set; }
     *     ....
     * }
     * 
     * Making A Custom YAML converter
     * 
     * public enum MyType { ... }
     * pbulic class MyTypeConverter : IYamlTypeConverter
     * {
     *     public bool Accepts(Type type)
     *     {
     *         return type == typeof(MyType);
     *     }
     *     
     *     public object? ReadYaml(IParser parser, Type type)
     *     {
     *         var value = ((Scalar)parser.Current).Value;
     *         parser.MoveNext();
     *         return Enum.Parse(typeof(MyType), value, true);
     *     }
     *     
     *     public void WriteYaml(IEmitter emitter, object? value, Type type)
     *     {
     *         emitter.Emit(new Scalar(null, null, value.ToString(), ScalarStyle.Any, true, false));
     *     }
     * }
     * 
     * 
     * 
     * 
     */
}
