declare module "token" {
    export enum tokenType {
        number = 0,
        character = 1,
        logical = 2,
        factor = 3,
        symbol = 4,
        operator = 5,
        comment = 6,
        newLine = 7,
        whitespace = 8
    }
    export interface token {
        text: string;
        type: tokenType;
    }
    export const logical: {};
    export const operators: {};
}
declare module "parser" {
    import { token } from "token";
    export class TokenParser {
        source: string;
        escaped: boolean;
        escape_char: string | null;
        escape_comment: boolean;
        /**
         * for get char at index
        */
        i: number;
        str_len: number;
        /**
         * the token text buffer
        */
        buf: string[];
        constructor(source: string);
        getTokens(): token[];
        private walkChar;
        private measureToken;
    }
}
declare module "app" {
    import { token } from "token";
    export function parseText(str: string): token[];
    /**
     * parse the script text to syntax highlight html content
    */
    export function highlights(str: string): string;
}
