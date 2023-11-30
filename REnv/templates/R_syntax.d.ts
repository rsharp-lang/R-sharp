declare module "app" {
    export enum tokenType {
        number = 0,
        character = 1,
        logical = 2,
        factor = 3,
        symbol = 4,
        operator = 5,
        comment = 6,
        newLine = 7
    }
    export interface token {
        text: string;
        type: tokenType;
    }
    export function parseText(str: string): void;
    export class TokenParser {
        source: string;
        escaped: boolean;
        escape_char: string | null;
        /**
         * for get char at index
        */
        i: number;
        str_len: number;
        constructor(source: string);
        getTokens(): token[];
    }
}
