///<reference path="token.ts" />

import { token, tokenType, logical, operators, keywords } from './token';

export class TokenParser {

    escaped: boolean = false;
    escape_char: string | null = null;
    escape_comment: boolean = false;

    /**
     * for get char at index
    */
    i: number = 0;
    str_len: number = -1;

    /**
     * the token text buffer
    */
    buf: string[] = [];

    public constructor(public source: string) {
        if (source) {
            this.str_len = source.length;
        }
    }

    public getTokens(): token[] {
        let tokens: token[] = [];
        let tmp: token = null;

        while (this.i < this.str_len) {
            if (tmp = this.walkChar(this.source.charAt(this.i++))) {
                tokens.push(tmp);
                this.buf = [];
            }
        }

        return tokens;
    }

    private walkChar(c: string): token {
        if (this.escaped) {
            this.buf.push(c);

            if (c == this.escape_char) {
                // end escape
                this.escaped = false;
                this.escape_char = null;

                return <token>{
                    text: this.buf.join(""),
                    type: tokenType.character
                }
            } else {
                // do nothing
            }

            return null;
        }

        if (this.escape_comment) {
            if (c == "\r" || c == "\n") {
                // end comment line
                this.escape_comment = false;

                return <token>{
                    text: this.buf.join(""),
                    type: tokenType.comment
                }
            } else {
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
            } else {
                this.buf.push(c);
            }
        } else if (c == "'" || c == '"') {
            // start string
            this.escape_char = c;
            this.escaped = true;

            if (this.buf.length > 0) {
                // populate previous token
                return this.measureToken();
            } else {
                this.buf.push(c);
            }
        } else if (c == " " || c == "\t") {
            if (this.buf.length > 0) {
                // populate previous token
                return this.measureToken();
            } else {
                return <token>{
                    type: tokenType.whitespace,
                    text: c
                };
            }
        }

        return null;
    }

    private measureToken(): token {
        var text: string = this.buf.join("");

        if (text == "NULL" || text == "NA") {
            return <token>{
                text: text,
                type: tokenType.factor
            }
        } else if (text in logical) {
            return <token>{
                text: text,
                type: tokenType.logical
            }
        } else if (text in keywords) {
            return <token>{
                text: text,
                type: tokenType.keyword
            }
        } else if (text.match(/[a-zA-Z_\.]/ig).index == 0) {
            // symbol
            return <token>{
                text: text,
                type: tokenType.symbol
            }
        } else if (text in operators) {
            return <token>{
                text: text,
                type: tokenType.operator
            }
        } else {
            return <token>{
                text: text,
                type: tokenType.number
            }
        }
    }
}