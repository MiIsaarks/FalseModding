using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.Options;
using System.Runtime.CompilerServices;


namespace ForgottenSkillsTweaks
{
    public static class config
    {

        public static event Action OnConfigChanged;

        public static ConfigEntry<bool> LightsOut;
        public static ConfigEntry<bool> Blast;
        public static ConfigEntry<bool> Dagger;
        public static ConfigEntry<bool> FlameThrower;
        public static ConfigEntry<bool> ArrowRain;
        public static ConfigEntry<bool> Blink;
        public static ConfigEntry<bool> LaserFather;
        public static ConfigEntry<bool> HH44;
        public static ConfigEntry<bool> PolarMines;
        public static ConfigEntry<bool> Blight;
        public static void init(ConfigFile config)
        {
            LightsOut = config.Bind(
               "Bandit",
               "Lights Out",
               true,
               "Enables the Lights Out changes."
            );

            Blast = config.Bind(
              "Bandit",
              "Blast",
              true,
              "Enables the Blast changes."
           );

            Dagger = config.Bind(
              "Bandit",
              "Serrated Dagger",
              true,
              "Enables the Serrated Dagger changes."
           );

            FlameThrower = config.Bind(
              "Artificer",
              "Flame Thrower",
              true,
              "Enables the Flame Thrower changes."
            );

            ArrowRain = config.Bind(
              "Huntress",
              "Arrow Rain",
              true,
              "Enables the Arrow Rain changes"
            );

            Blink = config.Bind(
              "Huntress",
              "Blink",
              true,
              "Enables the Blink changes."
            );

            LaserFather = config.Bind(
              "False Son",
              "Laser of The Father",
              true,
              "Enables the Laser of The Father changes"
            );

            HH44 = config.Bind(
              "RailGunner",
              "HH44",
              true,
              "Enables the HH44 changes"
            );

            PolarMines = config.Bind(
              "RailGunner",
              "Polar Mines",
              true,
              "Enables the Polar Mines changes"
            );

            Blight = config.Bind(
              "Acrid",
              "Blight",
              true,
              "Enables the Blight changes"
            );

            BindSettingChanged(LightsOut);
            BindSettingChanged(Blast);
            BindSettingChanged(Dagger);
            BindSettingChanged(FlameThrower);
            BindSettingChanged(ArrowRain);
            BindSettingChanged(Blink);
            BindSettingChanged(LaserFather);
            BindSettingChanged(HH44);
            BindSettingChanged(PolarMines);
            BindSettingChanged(Blight);



            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions"))
            {
                AddConfig();
            }
        }

        private static void BindSettingChanged(ConfigEntry<bool> entry)
        {
            entry.SettingChanged += (sender, args) =>
            {
                OnConfigChanged?.Invoke();
            };
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void AddConfig()
        {
            ModSettingsManager.AddOption(new CheckBoxOption(LightsOut));
            ModSettingsManager.AddOption(new CheckBoxOption(Blast));
            ModSettingsManager.AddOption(new CheckBoxOption(Dagger));
            ModSettingsManager.AddOption(new CheckBoxOption(FlameThrower));
            ModSettingsManager.AddOption(new CheckBoxOption(ArrowRain));
            ModSettingsManager.AddOption(new CheckBoxOption(Blink));
            ModSettingsManager.AddOption(new CheckBoxOption(LaserFather));
            ModSettingsManager.AddOption(new CheckBoxOption(HH44));
            ModSettingsManager.AddOption(new CheckBoxOption(PolarMines));
            ModSettingsManager.AddOption(new CheckBoxOption(Blight));
        }
    }
}

