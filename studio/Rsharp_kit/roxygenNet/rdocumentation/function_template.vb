Module function_template

    Friend Function blankTemplate() As XElement
        Return <html>
                   <head>
                       <!-- Viewport mobile tag for sensible mobile support -->
                       <meta name="viewport" content="width=device-width, initial-scale=1, maximum-scale=1"/>
                       <meta http-equiv="Content-Type" content="text/html" charset="UTF-8"/>
                       <meta name="description" content="{$abstract}"/>

                       <title>{$name_title} function | R Documentation</title>

                       <base href="https://www.rdocumentation.org"/>
                       <link href="{$canonical_link}" rel="canonical"/>

                       <!--STYLES-->
                       <link rel="stylesheet" href="/min/production.min.702e152d1c072db370ae8520b7e2d417.css"/>
                       <link href='https://fonts.googleapis.com/css?family=Open+Sans:400,300,300italic,400italic,600,600italic,700,700italic' rel='stylesheet' type='text/css'/>
                       <link rel="stylesheet" href="https://cdn.jsdelivr.net/simplemde/latest/simplemde.min.css"/>
                       <link rel="stylesheet" href='/css/nv.d3.min.css'/>
                       <link rel="stylesheet" href='/css/bootstrap-treeview.css'/>
                       <link rel="stylesheet" href='/css/bootstrap-glyphicons.css'/>
                       <!--STYLES END-->
                   </head>

                   <body>
                       <div id="content">

                           <section class="navbar navbar-color navbar-fixed-top">
                               <nav class="clearfix">
                                   <div class="navbar--title">
                                       <a href="/">
                                           <div class="logo"></div>
                                           <div class="logo-title"><span>RDocumentation</span></div>
                                       </a>
                                   </div>
                                   <ul class="navbar--navigation largescreen">
                                       <li>
                                           <a href="/login?rdr=%2Fpackages%2Fregtomean%2Fversions%2F1.0%2Ftopics%2Flanguage_test" class="btn btn-primary">Sign in</a>
                                       </li>
                                   </ul>

                                   <div class="navbar--search">
                                       <form class="search" action="/search" method="get">
                                           <input name="q" id="searchbar" type="text" placeholder="Search for packages, functions, etc" autocomplete="off"/>
                                           <input name="latest" id="hidden_latest" type="hidden"/>
                                           <div class="search--results"></div>
                                       </form>
                                   </div>
                               </nav>
                           </section>

                           <div class="page-wrap">

                               <section class="topic packageData" data-package-name="{$package}" data-latest-version="1.0" data-dcl='false'>

                                   <header class='topic-header'>
                                       <div class="container">

                                           <div class="th--flex-position">
                                               <div>
                                                   <!-- Do not remove this div, needed for th-flex-position -->
                                                   <h1>{$name_title}</h1>
                                               </div>
                                               <div>
                                                   <!-- Do not remove this div, needed for th-flex-position -->
                                                   <div class="th--pkg-info">
                                                       <div class="th--origin">
                                                           <span>From <a href="/packages/{$package}/versions/{$version}">{$package}</a></span>
                                                           <span>by <a href="/collaborators/name/{$author}">{$author}</a></span>
                                                       </div>
                                                       <div class="th--percentile">
                                                           <div class="percentile-widget percentile-task" data-url="/api/packages/regtomean/percentile">
                                                               <span class="percentile-th">
                                                                   <span class='percentile'>0th</span>
                                                               </span>
                                                               <p>Percentile</p>
                                                           </div>
                                                       </div>
                                                   </div>
                                               </div>
                                           </div>

                                       </div>
                                   </header>

                                   <div class="container">
                                       <section>
                                           <h5>{$title}</h5>
                                           <p>{$summary}</p>
                                       </section>

                                       <section class="topic--keywords" style="display: {$display_keywords};">
                                           <div class="anchor" id="l_keywords"></div>
                                           <dl>
                                               <dt>Keywords</dt>
                                               <dd><a href="/search/keywords/datasets">datasets</a></dd>
                                           </dl>
                                       </section>

                                       <section id="usage">
                                           <div class="anchor" id="l_usage"></div>
                                           <h5 class="topic--title">Usage</h5>
                                           <pre><code class="R">{$usage}</code></pre>
                                       </section>

                                       <!-- Other info -->
                                       <div class="anchor" id="l_sections"></div>

                                       <section>
                                           <h5 class="topic--title">Arguments</h5>
                                           <dl>
                                               {$arguments}
                                           </dl>
                                       </section>

                                       <section>
                                           <div class="anchor" id="l_details"></div>
                                           <h5 class="topic--title">Details</h5>
                                           <p>{$details}</p>
                                       </section>

                                       <section class="topic--value">
                                           <div class="anchor" id="l_value"></div>
                                           <h5 class="topic--title">Value</h5>
                                           <p>{$value}</p>
                                       </section>

                                       <section class="topic--examples">
                                           <div class="anchor" id="l_examples"></div>
                                           <h5 class="topic--title">Examples</h5>
                                           <p><pre><code>{$examples}</code></pre></p>
                                       </section>

                                       <section style="display: none;">
                                           <div class="anchor" id="alss"></div>
                                           <h5 class="topic--title">Aliases</h5>
                                           <ul class="topic--aliases">
                                               <li>{$name_title}</li>
                                           </ul>
                                       </section>

                                       <small>
                                           <i>Documentation reproduced from package <span itemprop="name">{$package}</span>, version <span itemprop="version">{$version}</span>, License: {$copyright}</i>
                                       </small>
                                   </div>
                               </section>

                           </div>

                           <div class="footer">
                               <div class="navbar--title footer-largescreen pull-right">

                                   <a href="https://github.com/SMRUCC/R-sharp" class="js-external">
                                       <div class="github"></div>
                                       <div class="logo-title">R# language</div>
                                   </a>

                               </div>
                               <div class="footer--credits--title">
                                   <p class="footer--credits">Created by <a href="https://github.com/SMRUCC/R-sharp" class="js-external">roxygenNet for R# language</a></p>
                               </div>
                           </div>

                       </div>
                   </body>
               </html>
    End Function
End Module
