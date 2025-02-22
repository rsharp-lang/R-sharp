// export R# source type define for javascript/typescript language
//
// package_source=REnv

declare namespace REnv {
   module _ {
      /**
      */
      function onLoad(): object;
   }
   /**
     * @param outputdir default value Is ``./``.
   */
   function __RSymbolDocumentation(symbols: any, package: any, outputdir?: any): object;
   /**
   */
   function __sourcescript(source: any): object;
   /**
   */
   function __template(): object;
   /**
   */
   function classify_cancer(): object;
   /**
   */
   function code_stats(stats: any, proj_folder: any, save: any): object;
   /**
     * @param row.names default value Is ``true``.
   */
   function coerce_dataframe(x: any, row.names?: any): object;
   /**
     * @param k default value Is ``6``.
     * @param qcut default value Is ``0.1``.
     * @param f default value Is ``10``.
   */
   function density2DCut(data: any, k?: any, qcut?: any, f?: any): object;
   /**
     * @param interval default value Is ``3``.
     * @param filetype default value Is ``html``.
   */
   function getHtml(url: any, interval?: any, filetype?: any): object;
   /**
     * @param interval default value Is ``3``.
   */
   function getImage(url: any, interval?: any): object;
   /**
     * @param interval default value Is ``3``.
     * @param raw_text default value Is ``false``.
   */
   function getJSON(url: any, interval?: any, raw_text?: any): object;
   github_url: string;
   /**
     * @param interval default value Is ``3``.
     * @param filetype default value Is ``html``.
   */
   function http_get(url: any, streamTo: any, interval?: any, filetype?: any): object;
   /**
     * @param direction default value Is ``1``.
   */
   function paletteer_c(palette: any, n: any, direction?: any): object;
   /**
   */
   function paletteer_colors(palette: any): object;
   /**
   */
   function platformName(): object;
   /**
     * @param stat default value Is ``Call "list"("totalLines" <- [],
     *       "commentLines" <- [],
     *       "blankLines" <- [],
     *       "size" <- [],
     *       "lineOfCodes" <- [],
     *       "classes" <- [],
     *       "method" <- [],
     *       "operator" <- [],
     *       "functions" <- [],
     *       "property" <- [],
     *       "files" <- [],
     *       "projList" <- [])``.
   */
   function process_project(vbproj: any, refer: any, banner: any, proj_folder: any, stat?: any): object;
   /**
     * @param stat default value Is ``Call "list"()``.
   */
   function processSingle(refer: any, banner: any, proj_folder: any, stat?: any): object;
   /**
   */
   function queryWeb(url: any, graphquery: any): object;
   /**
     * @param outputdir default value Is ``./``.
     * @param package default value Is ``null``.
   */
   function Rdocuments(pkgName: any, outputdir?: any, package?: any): object;
   /**
     * @param source default value Is ``null``.
     * @param debug default value Is ``false``.
     * @param workdir default value Is ``null``.
     * @param print_code default value Is ``false``.
   */
   function rlang_interop(code: any, source?: any, debug?: any, workdir?: any, print_code?: any): object;
   /**
   */
   function sacurine(): object;
   /**
   */
   function scale0_1(x: any): object;
   /**
     * @param source default value Is ``null``.
     * @param debug default value Is ``false``.
   */
   function transform_rlang_source(code: any, source?: any, debug?: any): object;
   /**
   */
   function walkFiles(vbproj: any, refer: any, banner: any, proj_folder: any): object;
}
