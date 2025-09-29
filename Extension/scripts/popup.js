const ytplus_Port = 10575;
const Engine = (typeof browser == 'undefined') ? 'chrome' : 'browser';
const Socket = new WebSocket(`ws://localhost:${ytplus_Port}/Websocket/`);

Socket.onopen = () => {
    Server.Send('Fetch-Downloads');
};
Socket.onmessage = (event) => {
    let Message = JSON.parse(event.data);
    let Payload = Message.Payload;
    switch (Message.Action)
    {
        case 'Load': Settings.Load(Payload); break;
        case 'AddDownload': UI.AddDownload(Payload.Index, Payload.Title, Payload.Source); break;
        case 'DownloadUpdate': UI.UpdateDownload(Payload.Download_ID, Payload.Progress); break;
        case 'DestinationUpdate': UI.UpdateDestination(Payload.Download_ID, Payload.Destination); break;
        case 'Fetch-Downloads':
        {
            let downloads = JSON.parse(Payload.Downloads);
            $.each(downloads, function(i, item) {
                UI.AddDownload(item.ID, item.Title, item.Source);
                UI.UpdateDownload(item.ID, item.Progress);
                UI.UpdateDestination(item.ID, item.Destination);
            });
        }
        break;
        default: console.log(`Unknown action: ${Action}`); break;
    }
};
Socket.onclose = () => {};
Socket.onerror = (error) => {
    $('#Connection-Error').css('display', 'flex');
};

$('#Settings').click(() => UI.SwitchTab('Settings'));
$('#Save-Settings').click(() => Settings.Save());
$('#Download-This').click(function()
{
    window[Engine].tabs.query({ active: true, currentWindow: true })
    .then((tabs) => 
    {
        const activeTabUrl = tabs[0].url;
        const url = new URL(activeTabUrl);
        let hostname = url.hostname.replace(/^www\./, '');
        console.log('[yt-plus]: ', activeTabUrl);

        /* Possible other domains as well, might turn this into an array */
        if (hostname == 'youtube.com' || hostname == 'youtu.be')
        {
            window[Engine].runtime.sendMessage({ Action: 'Get-Video-Data' })
            .then((response) => 
            {
                const Title = response.Title || activeTabUrl;
                const Source = response.Channel || hostname;
                console.log('[yt-plus]: ', Title, ' ', Source);
                Extension.Download(activeTabUrl, Title, Source);
            })
            .catch((error) => {
                console.error('Error getting video data:', error);
                Extension.Download(activeTabUrl, activeTabUrl, hostname);
          });
        }
        else
        {
            Extension.Download(activeTabUrl, activeTabUrl, hostname);
        }
    })
    .catch((error) => {
        console.error("Error getting active tab URL:", error);
    });
});

// Initialize dynamic UI functions
$(document).ready(function() {
    UI.FinishDownload();
    UI.OpenInFiles();
    UI.CancelDownload();
});

const UI =
{
    SwitchTab: function(page)
    {
        $('.Active-Tab').removeClass('Active-Tab');
        $(`.${page}-Page`).addClass('Active-Tab');
    },
    AddDownload: function(index, title, source)
    {
        let Item = `<div class="Item" index=${index}>
        <div class="Item-Wrap"><div class="Item-Video">
        <label class="Item-Title">${title}</label>
        <label class="Item-Channel">${source}</label>
        </div><div class="Item-Controls">
        <div class="Item-Control Item-Open-Files"><svg viewBox="0 0 16 16"><path d="m 2.96875 1.003906 c -1.644531 0 -3 1.355469 -3 3 v 8 c 0 1.644532 1.355469 3 3 3 h 3 c 1 0 1 -1 1 -1 s 0 -1 -1 -1 h -3 c -0.5625 0 -1 -0.4375 -1 -1 v -7 h 11 c 0.5625 0 1 0.4375 1 1 v 1 c 0 1 1 1 1 1 s 1 0 1 -1 v -1 c 0 -1.644531 -1.355469 -3 -3 -3 h -3.585938 l -1.707031 -1.707031 c -0.1875 -0.1875 -0.441406 -0.292969 -0.707031 -0.292969 z m 0 2 h 3.585938 l 1 1 h -5.585938 c 0 -0.5625 0.4375 -1 1 -1 z m 8.5 5 c -1.921875 0 -3.5 1.578125 -3.5 3.5 s 1.578125 3.5 3.5 3.5 c 0.597656 0 1.164062 -0.15625 1.660156 -0.425781 l 1.132813 1.132813 c 0.390625 0.390624 1.023437 0.390624 1.414062 0 c 0.390625 -0.390626 0.390625 -1.023438 0 -1.414063 l -1.132812 -1.128906 c 0 -0.003907 0 -0.003907 0 -0.003907 c 0.269531 -0.496093 0.425781 -1.058593 0.425781 -1.660156 c 0 -1.921875 -1.578125 -3.5 -3.5 -3.5 z m 0 2 c 0.839844 0 1.5 0.660156 1.5 1.5 c 0 0.40625 -0.15625 0.769532 -0.410156 1.035156 c -0.007813 0.007813 -0.019532 0.015626 -0.03125 0.027344 c -0.007813 0.007813 -0.015625 0.019532 -0.023438 0.027344 c -0.269531 0.253906 -0.628906 0.410156 -1.035156 0.410156 c -0.839844 0 -1.5 -0.660156 -1.5 -1.5 s 0.660156 -1.5 1.5 -1.5 z m 0 0"/></svg></div>
        <div class="Item-Control Item-Cancel"><svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16"><path d="m 5.019531 4 c -0.265625 0 -0.519531 0.105469 -0.707031 0.292969 c -0.390625 0.390625 -0.390625 1.023437 0 1.414062 l 2.292969 2.292969 l -2.292969 2.292969 c -0.390625 0.390625 -0.390625 1.023437 0 1.414062 s 1.023438 0.390625 1.414062 0 l 2.292969 -2.292969 l 2.292969 2.292969 c 0.390625 0.390625 1.023438 0.390625 1.414062 0 c 0.390626 -0.390625 0.390626 -1.023437 0 -1.414062 l -2.292968 -2.292969 l 2.292968 -2.292969 c 0.390626 -0.390625 0.390626 -1.023437 0 -1.414062 c -0.1875 -0.1875 -0.441406 -0.292969 -0.707031 -0.292969 s -0.519531 0.105469 -0.707031 0.292969 l -2.292969 2.292969 l -2.292969 -2.292969 c -0.1875 -0.1875 -0.441406 -0.292969 -0.707031 -0.292969 z m 0 0"/></svg></div>
        <div class="Item-Control Item-Done"><svg viewBox="0 0 16 16"><path d="m 13.753906 4.65625 c 0.175782 -0.199219 0.261719 -0.460938 0.246094 -0.722656 c -0.019531 -0.265625 -0.140625 -0.511719 -0.339844 -0.6875 c -0.199218 -0.175782 -0.460937 -0.265625 -0.726562 -0.246094 c -0.265625 0.015625 -0.511719 0.140625 -0.6875 0.339844 l -6.296875 7.195312 l -2.242188 -2.246094 c -0.390625 -0.390624 -1.023437 -0.390624 -1.414062 0 c -0.1875 0.1875 -0.292969 0.445313 -0.292969 0.710938 s 0.105469 0.519531 0.292969 0.707031 l 3 3 c 0.195312 0.195313 0.464843 0.300781 0.742187 0.292969 c 0.273438 -0.011719 0.535156 -0.132812 0.71875 -0.34375 z m 0 0"/></svg></div>
        </div></div><div class="Item-Progress"><div class="Progress"></div></div></div>`;

        $('.Downloads-Page').append(Item);
        
        let DownloadsShown = $('Downloads-Page').hasClass('Active-Tab');
        if (!DownloadsShown) UI.SwitchTab('Downloads');
    },
    UpdateDownload: function(index, progress)
    {
        let $Parent = $(`.Item[index="${index}"]`);
        let Item = $Parent.find('.Progress');
        Item.css('width', progress);

        if (progress != '100%') {
            $Parent.removeClass('Completed');
        } else {
            $Parent.addClass('Completed');
        }
    },
    UpdateDestination: function(index, destination)
    {
        let Item = $(`.Item[index="${index}"] .Item-Open-Files`);
        Item.attr('path', destination);
    },
    RemoveDownload: function(index)
    {
        let Item = $(`.Item[index="${index}"]`);
        Item.remove();

        let ActiveDownloads = $('.Item').length;
        if (ActiveDownloads == 0) UI.SwitchTab('Blank');
    },
    FinishDownload: function()
    {
        $(document).on('click', '.Item-Done', function()
        {
            let $Parent = $(this).closest('.Item');
            let Download_ID = $Parent.attr('index');

            UI.RemoveDownload(Download_ID);
            let download_id = parseInt(Download_ID);
            Server.Send('Finish', { download_id });
        });
    },
    OpenInFiles: function()
    {
        $(document).on('click', '.Item-Open-Files', function() 
        {
            let $Parent = $(this).closest('.Item-Open-Files');
            let Path = $Parent.attr('path');
            
            Extension.Browse(Path);
        });
    },
    CancelDownload: function()
    {
        $(document).on('click', '.Item-Cancel', function()
        {
            let $Parent = $(this).closest('.Item');
            let Download_ID = $Parent.attr('index');

            Extension.Cancel(Download_ID);
            UI.RemoveDownload(Download_ID);
        });
    }
};

const Server =
{
    Send: function(action, payload = {})
    {
        const Message = {action, payload};
        Socket.send(JSON.stringify(Message));
    }
};

const Settings = 
{
    Load: function(settings)
    {
        $('#Proxy').val(settings.Proxy.trim());
        $('#Cookies').val(settings.Cookies.trim());
    },
    Save: function()
    {
        let Proxy = $('#Proxy').val().trim();
        let Cookies = $('#Cookies').val().trim();

        Server.Send('Save', { Proxy, Cookies });
        UI.SwitchTab('Blank');
    }
};

const Extension =
{
    Download: function(url, title, source)
    {
        Server.Send('Download', { url, title, source });
    },
    Browse: function(path)
    {
        Server.Send('Browse', { path });
    },
    Cancel: function(id)
    {
        let download_id = parseInt(id);
        Server.Send('Cancel', { download_id });
    }
};