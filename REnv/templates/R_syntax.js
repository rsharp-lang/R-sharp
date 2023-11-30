define("app", ["require", "exports"], function (require, exports) {
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
    })(tokenType = exports.tokenType || (exports.tokenType = {}));
    function parseText(str) {
    }
    exports.parseText = parseText;
    var TokenParser = /** @class */ (function () {
        function TokenParser(source) {
            this.source = source;
            this.escaped = false;
            this.escape_char = null;
            /**
             * for get char at index
            */
            this.i = 0;
            this.str_len = -1;
            if (source) {
                this.str_len = source.length;
            }
        }
        TokenParser.prototype.getTokens = function () {
            var tokens = [];
            while (this.i < this.str_len) {
            }
            return tokens;
        };
        return TokenParser;
    }());
    exports.TokenParser = TokenParser;
});
//# sourceMappingURL=R_syntax.js.map