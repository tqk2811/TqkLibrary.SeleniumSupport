var g_acc = {
	email: "{email}",
	pass: "{pass}",
	recovery: "{recovery}"
};
var step = 0;
var timeout = Number("{timeout}");
var intervalTime = Number("{intervalTime}");

window.addEventListener('load', function () {
	window.setInterval(RunLogin, intervalTime);
	window.setTimeout(closeChrome, timeout);
	if (window.location.href.includes("myaccount.google.com")) closeChrome();
	if (!window.location.href.includes("accounts.google.com")) closeChrome();
});
function RunLogin() {
	if (step >= 2 && window.location.href.includes("/disabled/explanation")) closeChrome();//account banned
	switch (step) {
		case 0: {
			if (document.body.innerHTML.includes(g_acc.email)) {
				step = -1;
				closeChrome();
			}
			let email = document.querySelector("input[id='identifierId']");
			let btn_emailnext = document.querySelector("[id='identifierNext'] button");
			if (email && btn_emailnext && !isHidden(email)) {
				email.value = g_acc.email;
				btn_emailnext.click();
				step++;
			}
			break;
		}
		case 1: {
			let pass = document.querySelector("input[name='password']");
			let btn_passnext = document.querySelector("[id='passwordNext'] button");
			if (pass && btn_passnext && !isHidden(pass)) {
				pass.value = g_acc.pass;
				btn_passnext.click();
				step++;
			}
			break;
		}
		case 2: {
			let recs = document.querySelectorAll("div[jsname='EBHGs']:not([id])");
			if (recs.length != 0) {
				recs[recs.length - 1].click();
				step++;
			}
			else {
				let rec_mail = document.querySelector("input[id='knowledge-preregistered-email-response']");
				if(rec_mail && !isHidden(rec_mail)){
					step++;
				}
			}
			break;
		}
		case 3: {
			let rec_mail = document.querySelector("input[id='knowledge-preregistered-email-response']");
			let rec_mail_next = document.querySelector("button[jsname='LgbsSe']");
			if (rec_mail && rec_mail_next && !isHidden(rec_mail)) {
				rec_mail.value = g_acc.recovery;
				rec_mail_next.click();
				step++;
			}
			break;
		}
	}
}
function closeChrome() {
	chrome.runtime.sendMessage("close_tab_call");
}
function isHidden(el) {
	return (el.offsetParent === null)
}