var UMoney =
{
	UMoneyOpenConfirmURL: function (URL)
	{
		//Utf8_unityObject = UTF8ToString(unityObjectName);
		//Utf8_unityMethod = UTF8ToString(unityMethodName);
		Utf8_URL = UTF8ToString(URL);

		//console.log(unityObjectName);
		//console.log(Utf8_unityObject);

		//console.log(unityMethodName);
		//console.log(Utf8_unityMethod);		

		let newWin = window.open(Utf8_URL, "Payment", "width=600,height=800");
	},
}

mergeInto(LibraryManager.library, UMoney);