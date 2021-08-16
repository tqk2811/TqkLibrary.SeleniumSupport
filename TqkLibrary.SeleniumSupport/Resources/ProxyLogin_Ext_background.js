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

chrome.proxy.settings.set({value: config,scope: "regular"}, function () {});
chrome.webRequest.onAuthRequired.addListener(function callbackFn(details) { return authCre; }, { urls: ["<all_urls>"] }, ['blocking']);