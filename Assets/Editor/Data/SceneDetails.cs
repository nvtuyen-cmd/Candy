using Leopotam.Ecs;
using UnityEngine.SceneManagement;

namespace Eran {
    class SceneDetails : IEcsAutoReset {
        public string Path;
        public Scene Scene;
        public bool SearchRequested;
        public bool SearchDone;
        public bool WasOpened;

        public void Reset() {
            Path = default;
            Scene = default;
            SearchRequested = default;
            SearchDone = default;
            WasOpened = default;
        }
    }
}