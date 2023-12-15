var Token;
(function (Token) {
    function renderTextSet(chars) {
        var set = {};
        for (var _i = 0, chars_1 = chars; _i < chars_1.length; _i++) {
            var char = chars_1[_i];
            set[char] = 1;
        }
        return set;
    }
    Token.renderTextSet = renderTextSet;
    Token.logical = renderTextSet(["true", "false", "TRUE", "FALSE", "True", "False"]);
    Token.keywords = renderTextSet(["imports", "from", "require", "if", "else", "for", "function", "let", "const", "return", "", "", "", "", "", "", ""]);
    Token.operators = renderTextSet(["+", "-", "*", "/", "\\", "!", "$", "%", "^", "&", "=", "<", ">", ":", "|", ";", ""]);
})(Token || (Token = {}));
///<reference path="token.ts" />
var TokenParser = /** @class */ (function () {
    function TokenParser(source) {
        this.source = source;
        this.escaped = false;
        this.escape_char = null;
        this.escape_comment = false;
        /**
         * for get char at index
        */
        this.i = 0;
        this.str_len = -1;
        /**
         * the token text buffer
        */
        this.buf = [];
        if (source) {
            this.str_len = source.length;
        }
    }
    TokenParser.prototype.getTokens = function () {
        var tokens = [];
        var tmp = null;
        while (this.i < this.str_len) {
            if (tmp = this.walkChar(this.source.charAt(this.i++))) {
                tokens.push(tmp);
                this.buf = [];
            }
        }
        return tokens;
    };
    TokenParser.prototype.walkChar = function (c) {
        if (this.escaped) {
            this.buf.push(c);
            if (c == this.escape_char) {
                // end escape
                this.escaped = false;
                this.escape_char = null;
                return {
                    text: this.buf.join(""),
                    type: "character"
                };
            }
            else {
                // do nothing
            }
            return null;
        }
        if (this.escape_comment) {
            if (c == "\r" || c == "\n") {
                // end comment line
                this.escape_comment = false;
                return {
                    text: this.buf.join(""),
                    type: "comment"
                };
            }
            else {
                this.buf.push(c);
            }
            return null;
        }
        if (c == "#") {
            // start comment
            this.escape_comment = true;
            if (this.buf.length > 0) {
                // populate previous token
                return this.measureToken();
            }
            else {
                this.buf.push(c);
            }
        }
        else if (c == "'" || c == '"') {
            // start string
            this.escape_char = c;
            this.escaped = true;
            if (this.buf.length > 0) {
                // populate previous token
                return this.measureToken();
            }
            else {
                this.buf.push(c);
            }
        }
        else if (c == " " || c == "\t") {
            if (this.buf.length > 0) {
                // populate previous token
                return this.measureToken();
            }
            else {
                return {
                    type: "whitespace",
                    text: c
                };
            }
        }
        else {
            this.buf.push(c);
        }
        return null;
    };
    TokenParser.prototype.measureToken = function () {
        var text = this.buf.join("");
        var test_symbol = text.match(/[a-zA-Z_\.]/ig);
        if (text == "NULL" || text == "NA" || text == "NaN" || text == "Inf") {
            return {
                text: text,
                type: "factor"
            };
        }
        else if (text in Token.logical) {
            return {
                text: text,
                type: "logical"
            };
        }
        else if (text in Token.keywords) {
            return {
                text: text,
                type: "keyword"
            };
        }
        else if (test_symbol && (test_symbol.length > 0)) {
            // symbol
            return {
                text: text,
                type: "symbol"
            };
        }
        else if (text in Token.operators) {
            return {
                text: text,
                type: "operator"
            };
        }
        else {
            return {
                text: text,
                type: "number"
            };
        }
    };
    return TokenParser;
}());
///<reference path="token.ts" />
///<reference path="parser.ts" />
function parseText(str) {
    var parser = new TokenParser(str);
    var tokens = parser.getTokens();
    return tokens;
}
/**
 * parse the script text to syntax highlight html content
*/
function highlights(str) {
    var html = "";
    for (var _i = 0, _a = parseText(str); _i < _a.length; _i++) {
        var t = _a[_i];
        switch (t.type) {
            case "newLine":
                html = html + "\n";
                break;
            case "whitespace":
            case "operator":
            case "symbol":
                html = html + t.text;
                break;
            default:
                html = html + ("<span class=\"" + t.type + "\">" + t.text + "</span>");
        }
    }
    return html;
}
//# sourceMappingURL=R_syntax.js.map