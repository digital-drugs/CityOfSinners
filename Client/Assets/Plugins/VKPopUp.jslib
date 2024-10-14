var vkPopUp =
{
	VKAsyncInit: function ( appId)
	{
		Utf8_AppId = UTF8ToString(appId);

		window.vkAsyncInit = function ()
		{
			VK.init({
				apiId: Utf8_AppId
			});
		};

		setTimeout(function ()
		{
			var el = document.createElement("script");
			el.type = "text/javascript";
			el.src = "https://vk.com/js/api/openapi.js?169";
			el.async = true;
			//document.getElementById("vk_api_transport").appendChild(el);
			document.getElementsByTagName('head')[0].appendChild(el);
		}, 0);
	},

	VKLogin: function (unityObjectName, unityMethodName)
	{
		Utf8_unityObject = UTF8ToString(unityObjectName);
		Utf8_unityMethod = UTF8ToString(unityMethodName);

		console.log(unityObjectName);
		console.log(Utf8_unityObject);

		console.log(unityMethodName);
		console.log(Utf8_unityMethod);

		var settings = 2;

		VK.Auth.login(callback,settings);

		function callback(response) {
			//console.log('bfore response');

			if (response.session) {
				
				/* Пользователь успешно авторизовался */
				SendMessage(Utf8_unityObject, Utf8_unityMethod, response.session.user.id);
				//SendMessage("VKSDK","VKAuthResult", response.session.user.id);
				
				if (response.settings) {
					
				}
			}
			else {
				/* Пользователь нажал кнопку Отмена в окне авторизации */
			}
		}
	},

	VKLogout: function ()
	{
		console.log('start logout');
		VK.Auth.logout(callback);		
		console.log('end logout');

		function callback(response) {
			console.log('bfore response');

			if (response.session) {
				console.log('response.session');

				console.log('user id ' + response.session.user.id);

				/* Пользователь успешно авторизовался */
				if (response.settings) {
					console.log('login succes');
					console.log('login data ' + response.settings);
				}
			}
			else {
				/* Пользователь нажал кнопку Отмена в окне авторизации */
			}
		}
	},	

	VKGetFriends: function (unityObjectName, unityMethodName, userId)
	{
		Utf8_unityObject = UTF8ToString(unityObjectName);
		Utf8_unityMethod = UTF8ToString(unityMethodName);

		var method ="friends.get";

		var params =
		{
			user_id: userId, count: 10, fields: 'nickname,photo_100,sex', v: '5.131'
		};

		VK.Api.call(method, params, callback);

		function callback(r)
		{
			console.log('callback');
			console.log(r);
			console.log(r.response);

			if (r.response)
			{
				var result = JSON.stringify(r.response);

				SendMessage(Utf8_unityObject, Utf8_unityMethod, result);
			}
		}
	}
}

mergeInto(LibraryManager.library, vkPopUp);