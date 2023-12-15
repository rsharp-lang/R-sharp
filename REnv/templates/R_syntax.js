define("app", ["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    function parseText(str) {
        var parser = new TokenParser(str);
        var tokens = parser.getTokens();
        return tokens;
    }
    exports.parseText = parseText;
    /**
     * parse the script text to syntax highlight html content
    */
    function highlights(str) {
        var html = "";
        for (var _i = 0, _a = parseText(str); _i < _a.length; _i++) {
            var t = _a[_i];
            switch (t.type) {
                case Token.tokenType.newLine:
                    html = html + "\n";
                    break;
                case Token.tokenType.whitespace,
                    Token.tokenType.operator,
                    Token.tokenType.symbol:
                    html = html + t.text;
                    break;
                default:
                    html = html + ("<span class=\"" + t.type + "\">" + t.text + "</span>");
            }
        }
        return html;
    }
    exports.highlights = highlights;
});
var Token;
(function (Token) {
    var tokenType;
    (function (tokenType) {
        tokenType[tokenType["number"] = 0] = "number";
        tokenType[tokenType["character"] = 1] = "character";
        tokenType[tokenType["logical"] = 2] = "logical";
        tokenType[tokenType["factor"] = 3] = "factor";
        tokenType[tokenType["keyword"] = 4] = "keyword";
        tokenType[tokenType["symbol"] = 5] = "symbol";
        tokenType[tokenType["operator"] = 6] = "operator";
        tokenType[tokenType["comment"] = 7] = "comment";
        tokenType[tokenType["newLine"] = 8] = "newLine";
        tokenType[tokenType["whitespace"] = 9] = "whitespace";
    })(tokenType = Token.tokenType || (Token.tokenType = {}));
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
                    type: Token.tokenType.character
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
                    type: Token.tokenType.comment
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
                    type: Token.tokenType.whitespace,
                    text: c
                };
            }
        }
        return null;
    };
    TokenParser.prototype.measureToken = function () {
        var text = this.buf.join("");
        if (text == "NULL" || text == "NA") {
            return {
                text: text,
                type: Token.tokenType.factor
            };
        }
        else if (text in Token.logical) {
            return {
                text: text,
                type: Token.tokenType.logical
            };
        }
        else if (text in Token.keywords) {
            return {
                text: text,
                type: Token.tokenType.keyword
            };
        }
        else if (text.match(/[a-zA-Z_\.]/ig).index == 0) {
            // symbol
            return {
                text: text,
                type: Token.tokenType.symbol
            };
        }
        else if (text in Token.operators) {
            return {
                text: text,
                type: Token.tokenType.operator
            };
        }
        else {
            return {
                text: text,
                type: Token.tokenType.number
            };
        }
    };
    return TokenParser;
}());
//# sourceMappingURL=R_syntax.js.map