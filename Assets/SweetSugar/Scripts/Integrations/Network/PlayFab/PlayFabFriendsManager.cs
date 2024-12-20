﻿// // ©2015 - 2024 Candy Smith
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

using System;
using System.Collections.Generic;
#if PLAYFAB
using PlayFab.ClientModels;
using PlayFab;
using UnityEngine;
#endif

namespace SweetSugar.Scripts.Integrations.Network
{
	public class PlayFabFriendsManager : IFriendsManager {

		public PlayFabFriendsManager () {
		}

		public void Logout () {//1.3.3
		}


		/// <summary>
		/// Gets the friends list.
		/// </summary>
		public  void GetFriends (Action<Dictionary<string,string>> Callback) {
			#if PLAYFAB
		PlayFab.ClientModels.GetFriendsListRequest request = new PlayFab.ClientModels.GetFriendsListRequest () {
			ExternalPlatformFriends = ExternalFriendSources.Facebook
		};

		PlayFabClientAPI.GetFriendsList (request, (result) => {
			Dictionary<string,string> dic = new Dictionary<string, string> ();
			foreach (var item in result.Friends) {
				dic.Add (item.FacebookInfo.FacebookId, item.FriendPlayFabId);
			}
			Callback (dic);
		}, (error) => {
			Debug.Log (error.ErrorDetails);
		});

			#endif
		}

		/// <summary>
		/// Place the friends on map.
		/// </summary>
		public  void PlaceFriendsPositionsOnMap (Action<Dictionary<string,int>> Callback) {
			#if PLAYFAB
		Debug.Log ("place friends");
		PlayFab.ClientModels.GetFriendLeaderboardRequest request = new PlayFab.ClientModels.GetFriendLeaderboardRequest () {
			StatisticName = "Level",
			ExternalPlatformFriends = ExternalFriendSources.Facebook
		};

		PlayFabClientAPI.GetFriendLeaderboard (request, (result) => {
			Dictionary<string,int> dic = new Dictionary<string, int> ();
			foreach (var item in result.Leaderboard) {
				dic.Add (item.PlayFabId, item.StatValue);
			}
			Callback (dic);
		}, (error) => {
			Debug.Log (error.ErrorDetails);
		});
			#endif
		}

		/// <summary>
		/// Gets the leadboard on level.
		/// </summary>
		



	}
}

