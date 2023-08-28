// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function transliterate(text) {
    const cyrillic = {
        'а': 'a', 'б': 'b', 'в': 'v', 'г': 'g', 'д': 'd', 'е': 'e', 'ё': 'Io', 'ж': 'zh', 'з': 'z',
        'и': 'i', 'й': 'y', 'к': 'k', 'л': 'l', 'м': 'm', 'н': 'n', 'о': 'o', 'п': 'p', 'р': 'r',
        'с': 's', 'т': 't', 'у': 'u', 'ф': 'f', 'х': 'kh', 'ц': 'ts', 'ч': 'ch', 'ш': 'sh', 'щ': 'shch',
        'ъ': '', 'ы': 'y', 'ь': '', 'э': 'e', 'ю': 'iu', 'я': 'ia',
        'А': 'A', 'Б': 'B', 'В': 'V', 'Г': 'G', 'Д': 'D', 'Е': 'E', 'Ё': 'Io', 'Ж': 'Zh', 'З': 'Z',
        'И': 'I', 'Й': 'Y', 'К': 'K', 'Л': 'L', 'М': 'M', 'Н': 'N', 'О': 'O', 'П': 'P', 'Р': 'R',
        'С': 'S', 'Т': 'T', 'У': 'U', 'Ф': 'F', 'Х': 'Kh', 'Ц': 'Ts', 'Ч': 'Ch', 'Ш': 'Sh', 'Щ': 'Shch',
        'Ъ': '', 'Ы': 'Y', 'Ь': '', 'Э': 'E', 'Ю': 'Iu', 'Я': 'Ia'
    };

    return text.replace(/[а-яёА-ЯЁ]/g, char => cyrillic[char] || char);
}
const subscriptionKey = '32b7c18a2b574a3689ffe941e16aa3f9';
const endpoint = 'https://api.cognitive.microsofttranslator.com/';

function translateText(text) {
    console.log("translate");
    const requestBody = [{
        text: text
    }];

    const url = `${endpoint}/translate?api-version=3.0&to=en`;

    fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Ocp-Apim-Subscription-Key': subscriptionKey,
            'Ocp-Apim-Subscription-Region': 'northeurope'
        },
        body: JSON.stringify(requestBody)
    })
        .then(response => response.json())
        .then(data => {
            const translatedText = data[0].translations[0].text;
            $("#placeCity").val(translatedText);
        })
        .catch(error => {
            console.error('Error:', error);
        });
}


function ChangeButtonDisplay(isOn, id) {
    if (isOn) {
        $(`#prev${id}`).attr("hidden", false);
        $(`#next${id}`).attr("hidden", false);
        $(`#indicators${id}`).attr("hidden", false);
    }
    else {
        $(`#prev${id}`).attr("hidden", true);
        $(`#next${id}`).attr("hidden", true);
        $(`#indicators${id}`).attr("hidden", true);
    }
}