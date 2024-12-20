// // ©2015 - 2024 Candy Smith
// // All rights reserved
// // Redistribution of this software is strictly not allowed.
// // Copy of this software can be obtained from unity asset store only.
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// // FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// // THE SOFTWARE.

using SweetSugar.Scripts.GUI.Boost;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SweetSugar.Scripts.GUI
{
    /// <summary>
    /// Reward icon for the Reward popup
    /// </summary>
    public class RewardIcon : MonoBehaviour
    {
        public Sprite[] sprites;
        public Image icon;
        public Transform iconHolder;
        public TextMeshProUGUI text;
        public TextMeshProUGUI rewardName;

        private void Awake()
        {
        }


        /// <summary>
        /// Set icon
        /// </summary>
        /// <param name="i"></param>
        public void SetIconSprite(int i)
        {
            icon.sprite = sprites[i];
            if (i == 0)
            {
            }
            else if (i == 1)
            {
            }
        }
    }
}
