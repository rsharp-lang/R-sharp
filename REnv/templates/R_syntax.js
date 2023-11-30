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
    })(tokenType = exports.tokenType || (exports.tokenType = {}));
});
///<reference path="token.ts" />
define("parser", ["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
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
define("app", ["require", "exports", "parser"], function (require, exports, parser_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    function parseText(str) {
        var parser = new parser_1.TokenParser(str);
        var tokens = parser.getTokens();
        return tokens;
    }
    function highlights(str) {
    }
});
//# sourceMappingURL=R_syntax.js.map