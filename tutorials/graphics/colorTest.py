import grDevices
import ggplot
import graphics2D

paper = grDevices::colors("paper", n = -1, character = True)
paper = sapply(paper, color -> `<div style="background-color: ${color}; color: white;">${color}</div>`)
paper = append(`${length(paper)} colors`, paper)

writeLines(paper, con = `${@dir}/colors.html`)
