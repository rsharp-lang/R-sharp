///<reference path="token.ts" />

import { token } from './token';

export class TokenParser {

    escaped: boolean = false;
    escape_char: string | null = null;

    /**
     * for get char at index
    */
    i: number = 0;
    str_len: number = -1;

    public constructor(public source: string) {
        if (source) {
            this.str_len = source.length;
        }
    }

    public getTokens(): token[] {
        const tokens: token[] = [];

        while (this.i < this.str_len) {

        }

        return tokens;
    }


}