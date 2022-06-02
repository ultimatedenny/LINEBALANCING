'use strict';

//circle start
var progressBar = document.querySelector('.e-c-progress');
var indicator = document.getElementById('e-indicator');
var pointer = document.getElementById('e-pointer');
var length = Math.PI * 2 * 100;

progressBar.style.strokeDasharray = length;

function update(value, timePercent) {
    var offset = -length - length * value / timePercent;

    progressBar.style.strokeDashoffset = offset;
    pointer.style.transform = 'rotate(' + 360 * value / timePercent + 'deg)';
};

//circle ends
var displayOutput = document.querySelector('.display-remain-time');
var pauseBtn = document.getElementById('pause');
var setterBtns = document.querySelectorAll('button[data-setter]');

var intervalTimer = void 0;
var timeLeft = void 0;
var wholeTime = 0; // manage this to set the whole time
var timeOut = 240;

var isPaused = false;
var isStarted = false;

// This property value is used to reset stopwatch
var isReset = false;

update(wholeTime, wholeTime);
displayTimeLeft(wholeTime);

function timer(seconds) {

    //counts time, takes seconds
    var remainTime = Date.now() - seconds * 1000;

    displayTimeLeft(seconds);

    intervalTimer = setInterval(function () {
       // timeLeft = Math.round((Date.now() - remainTime) / 1000);
        timeLeft = ((Date.now() - remainTime) / 1000).toFixed(1);

        if (timeLeft > timeOut) {

            clearInterval(intervalTimer);
            isStarted = false;

            setterBtns.forEach(function (btn) {
                btn.disabled = false;
                btn.style.opacity = 1;
            });

            displayTimeLeft(wholeTime);

            pauseBtn.classList.remove('pause');
            pauseBtn.classList.add('play');
            return;
        }

        displayTimeLeft(timeLeft);

    }, 100);
}


pauseBtn.addEventListener('click', pauseTimer);

function pauseTimer(event)
{
    //console.log("is start", isStarted);

    if (isStarted === false)
    {
        timer(wholeTime);

        isStarted = true;

        this.classList.remove('play');
        this.classList.add('pause');

        setterBtns.forEach(function (btn) {
            btn.disabled = true;
            btn.style.opacity = 0.5;
        });
    }
    else if (isPaused === true)
    {
    
     
        var currentUrl = window.location.href;
        var urlParameter = getAllUrlParams(currentUrl);

        // Get values from parameters
        var isOneByOne = urlParameter.isonebyone;
        if (isOneByOne === "false")
        {
            if (displayOutput.value === '00')
            {
                isReset = true;
            }
                //Add Hidef/Rita 2021 04 22
            else
            {
                isReset=false;
            }
        }
        else
        {
            if (displayOutput.value === '00')
                timeLeft = 0;
        }

        if (isReset) {
            timeLeft = 0;

            isReset = false;
        }

        this.classList.remove('play');
        this.classList.add('pause');
      
        timer(timeLeft);
        isPaused = isPaused ? false : true;
    }
    else {
        this.classList.remove('pause');
        this.classList.add('play');

        clearInterval(intervalTimer);
        isPaused = isPaused ? false : true;
        
    }
}

function displayTimeLeft(timeLeft) {

    //displays time on the input
    var seconds = timeLeft % 3600;

    var displayString = '' + (seconds < 10 ? '0' : '') + seconds;
    displayOutput.value = displayString;

    update(timeLeft, timeOut);
}

// Add event to ignore button
var ignoreBtn = document.getElementById('ignore');
ignoreBtn.addEventListener('click', resetStopwatchOnIgnoreButton);

// Add event to close modal stopwatch button
var closeModalStopwatch = document.getElementById('closeModalStopwatch');
closeModalStopwatch.addEventListener('click', resetStopwatchCloseModal);

function resetStopwatchCloseModal()
{
    isPaused = false;
    isStarted = false;

    reset();
}

function resetStopwatchOnIgnoreButton()
{
    isPaused = false;
    isStarted = false;

    reset();
}


function reset(event) {
    pauseBtn.classList.remove('pause');
    pauseBtn.classList.add('play');

    //console.log("timeLeft", timeLeft);
    //console.log("timeOut", timeOut);

    // Override time left to clear interval timer on timer function
    timeLeft = timeOut + 1;

    var currentUrl = window.location.href;
    var urlParameter = getAllUrlParams(currentUrl);

    // Get values from parameters
    var isOneByOne = urlParameter.isonebyone;
    if (isOneByOne === "false") {
        // Override value reset = true;
        if (displayOutput.value !== '')
            isReset = true;
    }

    clearInterval(intervalTimer);
    displayTimeLeft(wholeTime);

    pauseBtn.classList.remove('pause');
    pauseBtn.classList.add('play');
}