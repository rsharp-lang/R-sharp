declare module "token" {
    export enum tokenType {
        number = 0,
        character = 1,
        logical = 2,
        factor = 3,
        keyword = 4,
        symbol = 5,
        operator = 6,
        comment = 7,
        newLine = 8,
        whitespace = 9
    }
    export interface token {
        text: string;
        type: tokenType;
    }
    export function renderTextSet(chars: string[]): object;
    export const logical: {};
    export const keywords: {};
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
