let Socket = null;
let ytplus_Port = 10575;
const YoutubeMenuSelector = 'segmented-like-dislike-button-view-model';
const Download_Button = `<yt-button-view-model id="ytplus-DownloadWrap" class="ytd-menu-renderer">
<button-view-model id="ytplus-Download" class="yt-spec-button-view-model style-scope ytd-menu-renderer">
<button class="yt-spec-button-shape-next yt-spec-button-shape-next--tonal yt-spec-button-shape-next--mono yt-spec-button-shape-next--size-m yt-spec-button-shape-next--icon-leading yt-spec-button-shape-next--enable-backdrop-filter-experiment" title="Download video" style="" aria-label="Download video" aria-disabled="false">
<div aria-hidden="true" class="yt-spec-button-shape-next__icon" id="ytplus-ButtonWrap">
<span class="ytIconWrapperHost" style="width: 24px; height: 24px;">
<span class="yt-icon-shape yt-spec-icon-shape">
<div style="width: 100%; height: 100%; display: block; fill: currentcolor;">
<svg height="24px" viewBox="0 -960 960 960" width="24px" fill="#f1f1f1" id="ytplus-Svg">
<path d="m480-332.5-163.81-164 38.93-39.34 97 97v-333.08h55.96v333.08l97-97L644-496.5l-164 164ZM256.29-188.08q-28.38 0-48.3-19.91-19.91-19.92-19.91-48.33v-106.56h55.96v106.53q0 4.62 3.84 8.47 3.85 3.84 8.47 3.84h447.3q4.62 0 8.47-3.84 3.84-3.85 3.84-8.47v-106.53h55.96v106.56q0 28.41-19.91 48.33-19.92 19.91-48.3 19.91H256.29Z"/>
</svg>
</div>
</span>
</span>
</div>
<div class="yt-spec-button-shape-next__button-text-content" id="ytplus-Text">Download</div>
</button></button-view-model></yt-button-view-model>`;

function injectDownloadButton() {
    const YoutubeMenu = $(YoutubeMenuSelector);
    if (YoutubeMenu.length && !$('#ytplus-Download').length) {
        YoutubeMenu[0].insertAdjacentHTML('afterend', Download_Button);

        let dl_Button = $('#ytplus-Download');
        dl_Button.on('click', function() {
            if (dl_Button.is('[Downloading]')) return;
            let VideoURL = window.location.href.split('&')[0];
            let Title = $("h1.ytd-watch-metadata yt-formatted-string").text().trim() || "Unknown Title";
            let Channel = $("ytd-channel-name#channel-name a").text().trim() || "Unknown Channel";
            let CheckMark = 'M382-267.69 183.23-466.46 211.77-495 382-324.77 748.23-691l28.54 28.54L382-267.69Z';

            Socket = new WebSocket(`ws://localhost:${ytplus_Port}/Websocket/`);

            Socket.onopen = () => {
                Server.Send('Download', {url: VideoURL, title: Title, source: Channel});
                Socket.close();
                $('#ytplus-Svg').find('path').attr('d', CheckMark);
                $('#ytplus-Text').remove();
                $('#ytplus-ButtonWrap').attr('style', 'margin-left: 0; margin-right: 0;');
                dl_Button.attr('Downloading', '');
                setTimeout(() => {
                    $('#ytplus-DownloadWrap').remove();
                }, 5000);
            };
            Socket.onclose = () => {};
            Socket.onerror = () => {};
            Socket.onmessage = () => {};
        });
    }
}

const Server = {
    Send: function(action, payload = {}) {
        const Message = {action, payload};
        Socket.send(JSON.stringify(Message));
    }
};

const observer = new MutationObserver((mutations) => {
    mutations.forEach(() => {
        injectDownloadButton();
    });
});
observer.observe(document.body, {
    childList: true,
    subtree: true
});

injectDownloadButton();