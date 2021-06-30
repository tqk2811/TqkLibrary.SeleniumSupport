chrome.runtime.onMessage.addListener(function (message, sender, callback) {
	if (message && message == "close_tab_call") {
		chrome.tabs.remove(sender.tab.id, function () {});
	}
});