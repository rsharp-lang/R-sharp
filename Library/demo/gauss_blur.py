import filter

img = readImage(`${@dir}/1537192287563.jpg`)
img = filter::gauss_blur(img, levels = 500)

bitmap(img, file = `${@dir}/gauss_blur.png`)