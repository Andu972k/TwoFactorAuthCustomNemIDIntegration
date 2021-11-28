from bottle import route, run, get, view, post, request, static_file
import json

##############################
# returns the index.html page from the views folder
@get("/")
@view("index.html")
def do():
    return dict(the_company_name = "Super")#the dict will pass the name super to the web page


##############################


@route("/static/<filepath:re:.*\.js>")
def send_js(filepath):
    return static_file(filepath, root="js/")

@route("/static/<filepath:re:.*\.css>")
def send_css(filepath):
    return static_file(filepath, root="css/")

##############################


@post("/get-name-by-cpr")
def do():
    # Connect to db
    # Execute a SQL

    data_from_client = json.load(request.body)#Retrieves the body of the request as a json object
    print(data_from_client.get("cpr"))

    with open(f'data/{data_from_client.get("cpr")}.txt') as f:
        lines = f.readlines()
    
    print("##############")
    print(lines)

    return lines




run(host="127.0.0.1", port=4444, debug=True, reloader=True, server="paste")