define("token", ["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    var tokenType;
    (function (tokenType) {
        tokenType[tokenType["number"] = 0] = "number";
        tokenType[tokenType["character"] = 1] = "character";
        tokenType[tokenType["logical"] = 2] = "logical";
        tokenType[tokenType["factor"] = 3] = "factor";
        tokenType[tokenType["symbol"] = 4] = "symbol";
        tokenType[tokenType["operator"] = 5] = "operator";
        tokenType[tokenType["comment"] = 6] = "comment";
        tokenType[tokenType["newLine"] = 7] = "newLine";
        tokenType[tokenType["whitespace"] = 8] = "whitespace";
    })(tokenType = exports.tokenType || (exports.tokenType = {}));
    exports.logical = {
        "TRUE": 1,
        "true": 1,
        "FALSE": 1,
        "false": 1
    };
    exports.operators = {
        "+": 1, "-": 1, "*": 1, "\\": 1, "/": 1,
        "^": 1, "%": 1,
        "$": 1, "&": 1, "|": 1
    };
});
///<reference path="token.ts" />
define("parser", ["require", "exports", "token"], function (require, exports, token_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
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
                        type: token_1.tokenType.character
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
                        type: token_1.tokenType.comment
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
            }
            return null;
        };
        TokenParser.prototype.measureToken = function () {
            var text = this.buf.join("");
            if (text == "NULL" || text == "NA") {
                return {
                    text: text,
                    type: token_1.tokenType.factor
                };
            }
            else if (text in token_1.logical) {
                return {
                    text: text,
                    type: token_1.tokenType.logical
                };
            }
            else if (text.match(/[a-zA-Z_\.]/ig).index == 0) {
                // symbol
                return {
                    text: text,
                    type: token_1.tokenType.symbol
                };
            }
        };
        return TokenParser;
    }());
    exports.TokenParser = TokenParser;
});
define("app", ["require", "exports", "parser"], function (require, exports, parser_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    function parseText(str) {
        var parser = new parser_1.TokenParser(str);
        var tokens = parser.getTokens();
        return tokens;
    }
    exports.parseText = parseText;
    /**
     * parse the script text to syntax highlight html content
    */
    function highlights(str) {
        var tokens = parseText(str);
        var html = "";
        for (var _i = 0, tokens_1 = tokens; _i < tokens_1.length; _i++) {
            var t = tokens_1[_i];
        }
        return html;
    }
    exports.highlights = highlights;
});
//# sourceMappingURL=R_syntax.js.map