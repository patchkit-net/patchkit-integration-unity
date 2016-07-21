namespace PatchKit.Unity
{
    /// <summary>
    /// Instance of <see cref="Api"/> which can be accessed globally.
    /// Settings of instance are located in Plugins/PatchKit/Resources/PatchKit API Settings.asset
    /// </summary>
    public static class ApiInstance
    {
        private static Api _api;

        public static Api Instance
        {
            get
            {
                EnsureThatApiIsCreated();
                return _api;
            }
        }

        public static void EnsureThatApiIsCreated()
        {
            if (_api == null)
            {
                CreateAPI();
            }
        }

        private static void CreateAPI()
        {
            var connectionSettings = ApiInstanceSettings.GetConnectionSettings();

            _api = new Api(connectionSettings);
        }
    }
}