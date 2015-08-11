using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AR_Wrapper
{
    /// <summary>
    ///     Control the scene changing flow
    /// </summary>
    public class SimpleSceneController : MonoBehaviour
    {
        private static SimpleSceneController appController;
        private string currentSceneName;
        private string nextSceneName;
        private AsyncOperation resourceUnloadTask;
        private AsyncOperation sceneLoadTask;
        private SceneState sceneState;
        private UpdateDelegate[] updateDelegates;

        public static SimpleSceneController Instance
        {
            get
            {
                if (appController != null) return appController;
                var go = new GameObject(typeof (SimpleSceneController).ToString());
                appController = go.AddComponent<SimpleSceneController>();
               
                return appController;
            }
        }

        /// <summary>
        ///     Call the static instance and change the scene
        /// </summary>
        /// <param name="nextSceneName"></param>
        public void SwitchScene(string nextSceneName)
        {
            if (appController == null) return;
            if (appController.currentSceneName != nextSceneName)
            {
                appController.nextSceneName = nextSceneName;
            }
        }

        void Awake()
        {

            if(appController != null && appController != this)
                Destroy(this);
            DontDestroyOnLoad(gameObject);

            //Setup the singleton instance
            appController = this;

            //Setup the array of updateDelegates
            updateDelegates = new UpdateDelegate[(int) SceneState.Count];

            //Set each updateDelegate
            updateDelegates[(int) SceneState.Reset] = UpdateSceneReset;
            updateDelegates[(int) SceneState.Preload] = UpdateScenePreload;
            updateDelegates[(int) SceneState.Load] = UpdateSceneLoad;
            updateDelegates[(int) SceneState.Unload] = UpdateSceneUnload;
            updateDelegates[(int) SceneState.Postload] = UpdateScenePostload;
            updateDelegates[(int) SceneState.Ready] = UpdateSceneReady;
            updateDelegates[(int) SceneState.Run] = UpdateSceneRun;

            nextSceneName = "scene2";
            sceneState = SceneState.Reset;
            //camera.orthographicSize = Screen.height / 2;
        }

        protected void OnDestroy()
        {
            //Clean up all the updateDelegates
            if (updateDelegates != null)
            {
                for (var i = 0; i < (int) SceneState.Count; i++)
                {
                    updateDelegates[i] = null;
                }
                updateDelegates = null;
            }

            //Clean up the singleton instance
            if (appController != null)
            {
                appController = null;
            }
        }

        protected void OnDisable()
        {
        }

        protected void OnEnable()
        {
           
        }

        protected void Start()
        {
           
        }

        protected void Update()
        {
            if (updateDelegates[(int) sceneState] != null)
            {
                updateDelegates[(int) sceneState]();
            }
        }

        /// <summary>
        ///     attach the new scene controller to start cascade of loading
        /// </summary>
        private void UpdateSceneReset()
        {
            // run a gc pass
            GC.Collect();
            sceneState = SceneState.Preload;
        }

        /// <summary>
        ///     handle anything that needs to happen before loading
        /// </summary>
        private void UpdateScenePreload()
        {
            sceneLoadTask = Application.LoadLevelAsync(nextSceneName);
            sceneState = SceneState.Load;
        }

        /// <summary>
        ///     Show the loading screen until it's loaded
        /// </summary>
        private void UpdateSceneLoad()
        {
            // done loading?
            if (sceneLoadTask.isDone)
            {
                sceneState = SceneState.Unload;
            }
        }

        /// <summary>
        ///     Clean up unused resources by unloading them
        /// </summary>
        private void UpdateSceneUnload()
        {
            // cleaning up resources yet?
            if (resourceUnloadTask == null)
            {
                resourceUnloadTask = Resources.UnloadUnusedAssets();
            }
            else
            {
                // done cleaning up?
                if (resourceUnloadTask.isDone != true) return;
                resourceUnloadTask = null;
                sceneState = SceneState.Postload;
            }
        }

        /// <summary>
        ///     handle anything that needs to happen immediately after loading
        /// </summary>
        private void UpdateScenePostload()
        {
            currentSceneName = nextSceneName;
            sceneState = SceneState.Ready;
        }

        /// <summary>
        ///     handle anything that needs to happen immediately before running
        /// </summary>
        private void UpdateSceneReady()
        {
            // run a gc pass
            // if you have assets loaded in the scene that are
            // currently unused currently but may be used later
            // DON'T do this here
            GC.Collect();
            sceneState = SceneState.Run;
        }

        /// <summary>
        ///     Wait for scene change
        /// </summary>
        private void UpdateSceneRun()
        {
            if (currentSceneName != nextSceneName)
            {
                sceneState = SceneState.Reset;
            }
        }

        /// <summary>
        ///     SceneState Enum
        /// </summary>
        private enum SceneState
        {
            Reset,
            Preload,
            Load,
            Unload,
            Postload,
            Ready,
            Run,
            Count
        };

        private delegate void UpdateDelegate();
    }
}