using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public static class BuildScript
{
    private static readonly string[] scenes = new[]
    {
        "Assets/Scenes/ServerBootstrap.unity",
        "Assets/Scenes/LoginScene.unity",
        "Assets/Scenes/MenuScene.unity",
        "Assets/Scenes/BattleScene.unity",
        "Assets/Scenes/MatchmakingScene.unity"
    };

    [MenuItem("Build/Build Client and Server")]
    public static void BuildClientAndServer()
    {
        // Build do client normal
        BuildPlayerOptions clientOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "Builds/macOSClient/CCTRoyale.app",
            target = BuildTarget.StandaloneOSX,
            options = BuildOptions.None,
            subtarget = (int)StandaloneBuildSubtarget.Player
        };
        BuildPipeline.BuildPlayer(clientOptions);

        // Build do servidor (headless)
        BuildPlayerOptions serverOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "Builds/macOSServer",
            target = BuildTarget.StandaloneOSX,
            options = BuildOptions.None,
            subtarget = (int)StandaloneBuildSubtarget.Server
        };
        BuildPipeline.BuildPlayer(serverOptions);
    }

    [MenuItem("Build/Build Server Only")]
    public static void BuildServer()
    {
        BuildPlayerOptions serverOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "Builds/macOSServer",
            target = BuildTarget.StandaloneOSX,
            options = BuildOptions.None,
            subtarget = (int)StandaloneBuildSubtarget.Server
        };
        BuildPipeline.BuildPlayer(serverOptions);
    }
}
