const baseURL = 'http://127.0.0.1:9001/'
let JWToken;



$(window).on("message", function(e) {
    
    try {
        console.log('##################')
        console.log(e);
        const response = e['originalEvent'];
        
        console.log(response);

        if (!response['data']) {
            console.log("message missing");
            return;
        }

        const jwt = response['data'];
        JWToken = jwt;
        request(jwt);
        
    } catch (ex) {
        console.log(ex.message)
    }
});

function request(jwt)
{
    const url = `${baseURL}Authentication/Code`;
    
    $.ajax(
    {
        type: "post",
        url: url,
        data: JSON.stringify(jwt),
        contentType: "text/json"
    }).done(function (data)
    {
        console.log(data);
        $("input#code").val("");
        $("input#submitButton").prop('disabled', false);
        $("span#response").text("");
        $("div#modal").show();
    }).fail(function (data)
    {
        console.log(data);
    });

    console.log(JSON.stringify(jwt))
}

$("form#formSendCode").on("submit", function (e)
{
    e.preventDefault();
    const url = `${baseURL}Authentication/Auth`
    const code = $("input#code").val();
    $.ajax(
    {
        url: url,
        type: "post",
        data: JSON.stringify(new Authentication(JWToken, code)),
        contentType: "text/json"
    }).done(function (data)
    {
        const response = $("span#response");
        switch (data) {
            case 0:
                response.text("Successfully logged in");
                $("input#submitButton").prop('disabled', true);
                break;
            case 1:
                response.text("Wrong code");
                break;
            case 2:
                response.text("No more tries");
                $("input#submitButton").prop('disabled', true);

                break;
            default:
                break;
        }
    }).fail(function (data)
    {
        console.log(data);
    });
});

$("span.closeModal").on("click", function (e)
{
    $("div#modal").hide();
});

class Authentication {
    constructor(JWToken, Code) {
        this.JWToken = JWToken;
        this.Code = Code;
    }
}