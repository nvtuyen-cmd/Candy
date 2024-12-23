using Leopotam.Ecs;

namespace Eran {
    class SceneResult : IEcsAutoReset {
        public string PathNicified;

        public void Reset() {
            PathNicified = default;
        }
    }
}