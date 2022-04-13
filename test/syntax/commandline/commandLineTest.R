const WORKDIR = normalizePath(dirname(@script));
const files   = [
	"cancer_010.raw", "cancer_001.raw", "cancer_002.raw", "cancer_003.raw", 
	"cancer_004.raw", "cancer_005.raw", "cancer_006.raw", "cancer_007.raw", 
	"cancer_008.raw", "cancer_009.raw"
];

[@ioredirect FALSE]
@`docker run -it --rm -e "WINEDEBUG=-all" -v "${WORKDIR}/:/data"
	 chambm/pwiz-skyline-i-agree-to-the-vendor-licenses
	 wine
	 msconvert --filter "msLevel 1-2"            
			   --zlib
			   --mzXML ${paste(files)}
			   --singleThreaded
`;