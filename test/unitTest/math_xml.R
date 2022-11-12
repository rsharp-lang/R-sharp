require(symbolic);

xml = `

<math xmlns="http://www.w3.org/1998/Math/MathML">        
          <lambda>
            <bvar>
              <ci> F6P </ci>
            </bvar>
            <bvar>
              <ci> PGIKf6p </ci>
            </bvar>
            <bvar>
              <ci> PGIkcat </ci>
            </bvar>
            <bvar>
              <ci> G6P </ci>
            </bvar>
            <bvar>
              <ci> PGIKeq </ci>
            </bvar>
            <bvar>
              <ci> PGI1 </ci>
            </bvar>
            <bvar>
              <ci> PGIKg6p </ci>
            </bvar>
            <apply>
              <divide/>
              <apply>
                <times/>
                <apply>
                  <times/>
                  <ci> PGI1 </ci>
                  <ci> PGIkcat </ci>
                </apply>
                <apply>
                  <plus/>
                  <apply>
                    <divide/>
                    <apply>
                      <minus/>
                      <ci> F6P </ci>
                    </apply>
                    <apply>
                      <times/>
                      <ci> PGIKeq </ci>
                      <ci> PGIKg6p </ci>
                    </apply>
                  </apply>
                  <apply>
                    <divide/>
                    <ci> G6P </ci>
                    <ci> PGIKg6p </ci>
                  </apply>
                </apply>
              </apply>
              <apply>
                <plus/>
                <apply>
                  <plus/>
                  <cn type="integer"> 1 </cn>
                  <apply>
                    <divide/>
                    <ci> F6P </ci>
                    <ci> PGIKf6p </ci>
                  </apply>
                </apply>
                <apply>
                  <divide/>
                  <ci> G6P </ci>
                  <ci> PGIKg6p </ci>
                </apply>
              </apply>
            </apply>
          </lambda>
        </math>

`;

exp = symbolic::parse.mathml(xml);

str(exp);

