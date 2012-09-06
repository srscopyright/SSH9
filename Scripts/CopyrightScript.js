// =====COPYRIGHT=====
// github ssh copyright text123
// =====COPYRIGHT=====

function SaveSuccessfullMsg() {
    document.getElementById("flash").style.display = "block";
    document.getElementById("flash").innerHTML = "<div class='alert-message success'>Save copyright configuration successful!</div>";
    document.getElementById("hiddenresult").value = 'Save copyright configuration successful!';
    //alert(document.getElementById("hiddenresult").value);
    $('div[id^=flash]').fadeOut(6500);
}
function SaveUnSuccessfullMsg() {
    document.getElementById("flash").style.display = "block";
    document.getElementById("flash").innerHTML = "<div class='alert-message error'>Save unsuccessful! The repository URL is not correct!</div>";
    document.getElementById("hiddenresult").value = 'Save unsuccessful! The repository URL is not correct!';
    $('div[id^=flash]').fadeOut(6500);
}
function ForceFailMsg(msg) {
    document.getElementById("flash").style.display = "block";
    document.getElementById("flash").innerHTML = "<div class='alert-message error'>"+msg+"</div>"; 
    document.getElementById("hiddenresult").value = msg;
}
function IsRunningMsg() {
    document.getElementById("loading1").style.display = "";
    $('#msg-dialog').dialog('open');
    return false;
}
$(document).ready(function () {
    $('#CopyrightText').addClass('required');
});

