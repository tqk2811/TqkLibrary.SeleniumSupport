var config = {
	mode: "fixed_servers",
	rules: {
		singleProxy: {
			scheme: "http",
			host: "{host}",
			port: {port}
		},
		bypassList: ["localhost"]
	}
};
var authCre = {
	authCredentials: {
		username: "{username}",
		password: "{password}"
	}
};

chrome.proxy.settings.set({ value: config, scope: "regular" }, function () { });
chrome.proxy.onProxyError.addListener(function (details) {
	console.log(details);
});
chrome.webRequest.onAuthRequired.addListener(
	function callbackFn(details) {
		console.log(details);
		return authCre;
	},
	{ urls: ["<all_urls>"] },
	['blocking']
);