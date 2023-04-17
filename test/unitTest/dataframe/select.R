# Create DataFrame
df <- data.frame(
  id = c(10,11,12,13),
  name = c('sai','ram','deepika','sahithi'),
  gender = c('M','M','F','F'),
  dob = as.Date(c('1990-10-02','1981-3-24','1987-6-14','1985-8-16')),
  state = c('CA','NY','DE','NA'),
  row.names=c('r1','r2','r3','r4')
);

print(df);

print(df |> select('name','gender', id -> people_id));