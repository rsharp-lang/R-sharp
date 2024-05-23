         # create data frame
         df <- data.frame(team     = c('A', 'A', 'A', 'B', 'B', 'B'),
                             position = c('G', 'G', 'F', 'G', 'F', 'F'),
                             points   = c(99, 90, 86, 88, 95, 99),
                            assists  = c(33, 28, 31, 39, 34, 23),
                             rebounds = c(30, 28, 24, 24, 28, 33));
         
         # view data frame
        print(df);
         
        # find mean points by team
       print( aggregate(df$points, by=list(df$team), FUN=mean));
        
        #  Group.1        x
        # 1       A 91.66667
        # 2       B 94.00000
         
         # or 
        print(aggregate(df, by = points ~ team, FUN = mean));
         
        #   Group.1        x
        # 1       A 91.66667
        # 2       B 94.00000
         
         # get aggregate function demo
         #
         let f = aggregate(FUN = "mean");
         
         # is equalient as the expression mean
        print(f([1,2,3,4,5]));
        print(mean([1,2,3,4,5]));