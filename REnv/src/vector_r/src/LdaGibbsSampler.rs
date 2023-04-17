pub struct LdaGibbsSampler {
    pub mut documents: Vec<Vec<i32>>;
}

pub impl LdaGibbsSampler {
    

    fn gibbs(&self) {

    }
}

fn rect_array(width: usize, height: usize) {
    // Base 1d array
    let mut grid_raw = vec![0; width * height];
    // Vector of 'width' elements slices
    let mut grid_base: Vec<_> = grid_raw.as_mut_slice().chunks_mut(width).collect();
    // Final 2d array `&mut [&mut [_]]`
    let grid = grid_base.as_mut_slice();

    return grid;
}