// export R# source type define for javascript/typescript language
//
// package_source=REnv

declare namespace REnv {
   /**
   */
   function classify_cancer(): object;
   /**
     * @param k default value Is ``6``.
     * @param qcut default value Is ``0.1``.
     * @param f default value Is ``10``.
   */
   function density2DCut(data:any, k:object, qcut:number, f:object): object;
   /**
     * @param interval default value Is ``3``.
     * @param filetype default value Is ``"html"``.
   */
   function http_get(url:any, streamTo:any, interval:object, filetype:string): object;
   /**
     * @param interval default value Is ``3``.
   */
   function getImage(url:any, interval:object): object;
   /**
     * @param interval default value Is ``3``.
     * @param filetype default value Is ``"html"``.
   */
   function getHtml(url:any, interval:object, filetype:string): object;
   /**
     * @param interval default value Is ``3``.
     * @param raw_text default value Is ``False``.
   */
   function getJSON(url:any, interval:object, raw_text:boolean): object;
   /**
   */
   function scale0_1(x:any): object;
   /**
   */
   function queryWeb(url:any, graphquery:any): object;
   /**
   */
   function platformName(): object;
   /**
     * @param outputdir default value Is ``"./"``.
     * @param package default value Is ``NULL``.
   */
   function Rdocuments(pkgName:any, outputdir:string, package:any): object;
   module  {
      /**
      */
      function onLoad(): object;
   }
}
