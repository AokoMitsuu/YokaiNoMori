using System;
using UnityEngine;

namespace Core
{
    public static class GameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            GameObject app = UnityEngine.Object.Instantiate(Resources.Load("App")) as GameObject;

            if (app == null) { throw new ApplicationException(); }

            UnityEngine.Object.DontDestroyOnLoad(app);
        }
    }
}
