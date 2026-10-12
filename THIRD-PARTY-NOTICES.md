# Third-party notices

MailBox MatriX is licensed under the MIT License (see `LICENSE`). It is based on **QuickMail**
(https://github.com/kellylford/QuickMail), Copyright (c) 2026 Kelly Ford, also MIT. Its copyright
and permission notice are retained in `LICENSE`.

## Components shipped in `MailBoxMatriX.exe`

Verified on 2026-10-11 from the restored packages' `.nuspec` licence metadata
(`dotnet list QuickMail/QuickMail.csproj package --include-transitive`).

| Component | Version | Licence | Copyright (from the package metadata) |
|---|---|---|---|
| MailKit | 4.17.0 | MIT | .NET Foundation and Contributors |
| MimeKit | 4.17.0 | MIT | .NET Foundation and Contributors |
| BouncyCastle.Cryptography | 2.6.2 | MIT | Copyright © Legion of the Bouncy Castle Inc. 2000-2025 |
| Microsoft.Identity.Client (MSAL) | 4.89.0 | MIT | © Microsoft Corporation |
| Microsoft.Identity.Client.Extensions.Msal | 4.89.0 | MIT | © Microsoft Corporation |
| Microsoft.IdentityModel.Abstractions | 8.14.0 | MIT | © Microsoft Corporation |
| Microsoft.Data.Sqlite, .Core | 10.0.12 | MIT | © Microsoft Corporation |
| SQLitePCLRaw (bundle_e_sqlite3, core, lib, provider) | 2.1.12 | Apache-2.0 | Copyright 2014-2024 SourceGear, LLC |
| SQLite (inside e_sqlite3) | — | Public domain | — |
| Google.Apis, Google.Apis.Auth, Google.Apis.Core | 1.76.0 | Apache-2.0 | Copyright 2021 Google LLC |
| CommunityToolkit.Mvvm | 8.4.2 | MIT | (c) .NET Foundation and Contributors |
| AdysTech.CredentialManager | 3.1.0 | MIT | © Adys Tech 2016-2026 |
| Microsoft.Toolkit.Uwp.Notifications | 7.1.3 | MIT | (c) .NET Foundation and Contributors |
| Microsoft.Web.WebView2 (SDK) | 1.0.4191.47 | Microsoft BSD-3-style licence (`LICENSE.txt` in the package) | © Microsoft Corporation |
| Markdig | 1.3.2 | BSD-2-Clause | Alexandre Mutel |
| Newtonsoft.Json | 13.0.4 | MIT | Copyright © James Newton-King 2008 |
| System.Management | 7.0.2 | MIT | © Microsoft Corporation |
| Velopack (runtime) | 1.2.161 | MIT | Copyright © Velopack Ltd. |
| .NET runtime and WPF (self-contained) | 10.0.12 | MIT | .NET Foundation and contributors |

**Build-time only (not shipped):** Roslynator.Analyzers (Apache-2.0), Microsoft.NET.ILLink.Tasks (MIT).

**Not bundled:** the **Microsoft Edge WebView2 Runtime** is installed separately on Windows under
Microsoft's own terms.

**Removed by MailBoxMatriX:** `Microsoft.Identity.Client.Desktop`, `.Broker` and `.NativeInterop`.
NativeInterop's Microsoft Software License Terms restrict redistribution (§3e); see
`docs/AUTHENTICATION.md`.

## Obligations for any distributed build

- Include `LICENSE` and this file.
- **MIT, BSD:** reproduce each component's copyright and licence text. **Apache-2.0:** include the
  licence and any `NOTICE` file the package carries. The licence files are inside each package in the
  NuGet cache (`%USERPROFILE%\.nuget\packages\<id>\<version>\`). A release step should collect them
  into a `licenses\` folder next to the installer contents. This step is **not yet automated**
  (see `docs/MAILBOXX_BUILD.md`).
- WebView2 SDK: keep Microsoft's notice. Do not use Microsoft's name to endorse MailBox MatriX.

## Photographs

| File | Work | Author | Licence | Source | Changes |
|---|---|---|---|---|---|
| `QuickMail/Assets/Scenic/serengeti-kopjes.jpg` | *Tanzania (Serengeti National Park) Kopjes, the great granite mass, are an attractive feature of the Serengeti landscape* (8 September 2015) | Güldem Üstün, Istanbul (https://www.flickr.com/people/126397594@N03) | Creative Commons Attribution 2.0 Generic (CC BY 2.0), https://creativecommons.org/licenses/by/2.0/ | https://www.flickr.com/photos/guldem/21246555775/ via https://commons.wikimedia.org/wiki/File:Tanzania_(Serengeti_National_Park)_Kopjes,_the_great_granite_mass,_are_an_attractive_feature_of_the_Serengeti_landscape_(21246555775).jpg | Resized to 2560 × 1728 and re-encoded as JPEG |

The CC BY licences require this credit (author, title, licence and link) to accompany each image
wherever it is distributed, and an indication that it was changed.

### Row pictures

The contextual pictures behind message rows are photographs from Wikimedia Commons, chosen with
licences that allow redistribution and modification without share-alike or non-commercial terms
(CC0, public domain, CC BY). Each appears in a light and a dark version.

| File | Work | Author | Licence | Source | Changes |
|---|---|---|---|---|---|
| `QuickMail/Assets/ContextImages/{light,dark}/travel-1.jpg` | *A wing tip of an airplane (40118125441).jpg* | U.S. Department of Agriculture | Public domain | https://commons.wikimedia.org/wiki/File:A_wing_tip_of_an_airplane_(40118125441).jpg | Cropped to 3.2:1 and resized to 960 × 300; light set blended 50% toward white, dark set 65% toward #202124; re-encoded as JPEG |
| `QuickMail/Assets/ContextImages/{light,dark}/travel-2.jpg` | *Land Clouds And Sky (130308693).jpeg* | Abhijith Cudappah | Creative Commons Attribution 3.0 (CC BY 3.0), https://creativecommons.org/licenses/by/3.0/ | https://commons.wikimedia.org/wiki/File:Land_Clouds_And_Sky_(130308693).jpeg | Cropped to 3.2:1 and resized to 960 × 300; light set blended 50% toward white, dark set 65% toward #202124; re-encoded as JPEG |
| `QuickMail/Assets/ContextImages/{light,dark}/shopping-1.jpg` | *Mini grocery toy pushcart.jpg* | Me (Elsa Versailles) | Public domain | https://commons.wikimedia.org/wiki/File:Mini_grocery_toy_pushcart.jpg | Cropped to 3.2:1 and resized to 960 × 300; light set blended 50% toward white, dark set 65% toward #202124; re-encoded as JPEG |
| `QuickMail/Assets/ContextImages/{light,dark}/shopping-2.jpg` | *Shopping cart of Prisma shop.JPG* | Dmitry G | Public domain | https://commons.wikimedia.org/wiki/File:Shopping_cart_of_Prisma_shop.JPG | Cropped to 3.2:1 and resized to 960 × 300; light set blended 50% toward white, dark set 65% toward #202124; re-encoded as JPEG |
| `QuickMail/Assets/ContextImages/{light,dark}/finance-1.jpg` | *Belgium 1835 40 Francs.jpg* | Belgium (coin), National Numismatic Collection (image) | Public domain | https://commons.wikimedia.org/wiki/File:Belgium_1835_40_Francs.jpg | Cropped to 3.2:1 and resized to 960 × 300; light set blended 50% toward white, dark set 65% toward #202124; re-encoded as JPEG |
| `QuickMail/Assets/ContextImages/{light,dark}/finance-2.jpg` | *Danish West Indies 1904 10 Daler.jpg* | National Museum of American History | Public domain | https://commons.wikimedia.org/wiki/File:Danish_West_Indies_1904_10_Daler.jpg | Cropped to 3.2:1 and resized to 960 × 300; light set blended 50% toward white, dark set 65% toward #202124; re-encoded as JPEG |
| `QuickMail/Assets/ContextImages/{light,dark}/work-1.jpg` | *Desk-laptop-notebook-pen (23958719819).jpg* | www.Pixel.la Free Stock Photos | CC0 1.0 (public domain dedication), https://creativecommons.org/publicdomain/zero/1.0/ | https://commons.wikimedia.org/wiki/File:Desk-laptop-notebook-pen_(23958719819).jpg | Cropped to 3.2:1 and resized to 960 × 300; light set blended 50% toward white, dark set 65% toward #202124; re-encoded as JPEG |
| `QuickMail/Assets/ContextImages/{light,dark}/work-2.jpg` | *Desk-laptop-notebook-table (23697249344).jpg* | www.Pixel.la Free Stock Photos | CC0 1.0 (public domain dedication), https://creativecommons.org/publicdomain/zero/1.0/ | https://commons.wikimedia.org/wiki/File:Desk-laptop-notebook-table_(23697249344).jpg | Cropped to 3.2:1 and resized to 960 × 300; light set blended 50% toward white, dark set 65% toward #202124; re-encoded as JPEG |
| `QuickMail/Assets/ContextImages/{light,dark}/events-1.jpg` | *XS Nightclub Las Vegas Confetti 2.jpg* | jencu | Creative Commons Attribution 2.0 (CC BY 2.0), https://creativecommons.org/licenses/by/2.0/ | https://commons.wikimedia.org/wiki/File:XS_Nightclub_Las_Vegas_Confetti_2.jpg | Cropped to 3.2:1 and resized to 960 × 300; light set blended 50% toward white, dark set 65% toward #202124; re-encoded as JPEG |
| `QuickMail/Assets/ContextImages/{light,dark}/events-2.jpg` | *Balloon & Party Poppers (16346732182).jpg* | oatsy40 | Creative Commons Attribution 2.0 (CC BY 2.0), https://creativecommons.org/licenses/by/2.0/ | https://commons.wikimedia.org/wiki/File:Balloon_%26_Party_Poppers_(16346732182).jpg | Cropped to 3.2:1 and resized to 960 × 300; light set blended 50% toward white, dark set 65% toward #202124; re-encoded as JPEG |
| `QuickMail/Assets/ContextImages/{light,dark}/tech-1.jpg` | *Printed Circuit Board 9990.jpg* | Ashley Pomeroy | Creative Commons Attribution 4.0 (CC BY 4.0), https://creativecommons.org/licenses/by/4.0/ | https://commons.wikimedia.org/wiki/File:Printed_Circuit_Board_9990.jpg | Cropped to 3.2:1 and resized to 960 × 300; light set blended 50% toward white, dark set 65% toward #202124; re-encoded as JPEG |
| `QuickMail/Assets/ContextImages/{light,dark}/tech-2.jpg` | *Aurora 2ch mixer board arrived (2009-07-28 11.20.55 by c-g.).jpg* | c-g. | Creative Commons Attribution 2.0 (CC BY 2.0), https://creativecommons.org/licenses/by/2.0/ | https://commons.wikimedia.org/wiki/File:Aurora_2ch_mixer_board_arrived_(2009-07-28_11.20.55_by_c-g.).jpg | Cropped to 3.2:1 and resized to 960 × 300; light set blended 50% toward white, dark set 65% toward #202124; re-encoded as JPEG |
| `QuickMail/Assets/ContextImages/{light,dark}/food-1.jpg` | *Coffee cup in Hanoi, Vietnam.jpg* | Vyacheslav Argenberg | Creative Commons Attribution 4.0 (CC BY 4.0), https://creativecommons.org/licenses/by/4.0/ | https://commons.wikimedia.org/wiki/File:Coffee_cup_in_Hanoi,_Vietnam.jpg | Cropped to 3.2:1 and resized to 960 × 300; light set blended 50% toward white, dark set 65% toward #202124; re-encoded as JPEG |
| `QuickMail/Assets/ContextImages/{light,dark}/food-2.jpg` | *02024 1710 Vegetables of Silesia.jpg* | Silar | CC0 1.0 (public domain dedication), https://creativecommons.org/publicdomain/zero/1.0/ | https://commons.wikimedia.org/wiki/File:02024_1710_Vegetables_of_Silesia.jpg | Cropped to 3.2:1 and resized to 960 × 300; light set blended 50% toward white, dark set 65% toward #202124; re-encoded as JPEG |
| `QuickMail/Assets/ContextImages/{light,dark}/fitness-1.jpg` | *Kurzhanteln 2 x 15 kg 2v2.jpg* | Singlespeedfahrer | CC0 1.0 (public domain dedication), https://creativecommons.org/publicdomain/zero/1.0/ | https://commons.wikimedia.org/wiki/File:Kurzhanteln_2_x_15_kg_2v2.jpg | Cropped to 3.2:1 and resized to 960 × 300; light set blended 50% toward white, dark set 65% toward #202124; re-encoded as JPEG |
| `QuickMail/Assets/ContextImages/{light,dark}/fitness-2.jpg` | *Tennis racket and ball.JPG* | nao2g | Creative Commons Attribution 3.0 (CC BY 3.0), https://creativecommons.org/licenses/by/3.0/ | https://commons.wikimedia.org/wiki/File:Tennis_racket_and_ball.JPG | Cropped to 3.2:1 and resized to 960 × 300; light set blended 50% toward white, dark set 65% toward #202124; re-encoded as JPEG |
| `QuickMail/Assets/ContextImages/{light,dark}/photography-1.jpg` | *Agfa Click BW 1.JPG* | Berthold Werner | Public domain | https://commons.wikimedia.org/wiki/File:Agfa_Click_BW_1.JPG | Cropped to 3.2:1 and resized to 960 × 300; light set blended 50% toward white, dark set 65% toward #202124; re-encoded as JPEG |
| `QuickMail/Assets/ContextImages/{light,dark}/photography-2.jpg` | *Photographic lenses and tubes 2025.jpg* | Carlos Valenzuela Montuy | Creative Commons Attribution 4.0 (CC BY 4.0), https://creativecommons.org/licenses/by/4.0/ | https://commons.wikimedia.org/wiki/File:Photographic_lenses_and_tubes_2025.jpg | Cropped to 3.2:1 and resized to 960 × 300; light set blended 50% toward white, dark set 65% toward #202124; re-encoded as JPEG |
| `QuickMail/Assets/ContextImages/{light,dark}/news-1.jpg` | *Newspaper 20250330 120826.jpg* | Ka23 13 | Creative Commons Attribution 4.0 (CC BY 4.0), https://creativecommons.org/licenses/by/4.0/ | https://commons.wikimedia.org/wiki/File:Newspaper_20250330_120826.jpg | Cropped to 3.2:1 and resized to 960 × 300; light set blended 50% toward white, dark set 65% toward #202124; re-encoded as JPEG |
| `QuickMail/Assets/ContextImages/{light,dark}/news-2.jpg` | *Vecteezy stack-of-newspaper 1961329.jpg* | Bernerlover | Creative Commons Attribution 4.0 (CC BY 4.0), https://creativecommons.org/licenses/by/4.0/ | https://commons.wikimedia.org/wiki/File:Vecteezy_stack-of-newspaper_1961329.jpg | Cropped to 3.2:1 and resized to 960 × 300; light set blended 50% toward white, dark set 65% toward #202124; re-encoded as JPEG |
| `QuickMail/Assets/ContextImages/{light,dark}/security-1.jpg` | *Machermo, Key, Lock, Nepal.jpg* | Vyacheslav Argenberg | Creative Commons Attribution 4.0 (CC BY 4.0), https://creativecommons.org/licenses/by/4.0/ | https://commons.wikimedia.org/wiki/File:Machermo,_Key,_Lock,_Nepal.jpg | Cropped to 3.2:1 and resized to 960 × 300; light set blended 50% toward white, dark set 65% toward #202124; re-encoded as JPEG |
| `QuickMail/Assets/ContextImages/{light,dark}/security-2.jpg` | *Liebesschloss.jpg* | Schlurcher | Creative Commons Attribution 4.0 (CC BY 4.0), https://creativecommons.org/licenses/by/4.0/ | https://commons.wikimedia.org/wiki/File:Liebesschloss.jpg | Cropped to 3.2:1 and resized to 960 × 300; light set blended 50% toward white, dark set 65% toward #202124; re-encoded as JPEG |
| `QuickMail/Assets/ContextImages/{light,dark}/delivery-1.jpg` | *An Overview of Moving Companies and Their Use of Moving Boxes.jpg* | brownpau | Creative Commons Attribution 2.0 (CC BY 2.0), https://creativecommons.org/licenses/by/2.0/ | https://commons.wikimedia.org/wiki/File:An_Overview_of_Moving_Companies_and_Their_Use_of_Moving_Boxes.jpg | Cropped to 3.2:1 and resized to 960 × 300; light set blended 50% toward white, dark set 65% toward #202124; re-encoded as JPEG |
| `QuickMail/Assets/ContextImages/{light,dark}/delivery-2.jpg` | *Parcel delivery Venice Christmas 2012.jpg* | Sue elias | Creative Commons Attribution 2.0 (CC BY 2.0), https://creativecommons.org/licenses/by/2.0/ | https://commons.wikimedia.org/wiki/File:Parcel_delivery_Venice_Christmas_2012.jpg | Cropped to 3.2:1 and resized to 960 × 300; light set blended 50% toward white, dark set 65% toward #202124; re-encoded as JPEG |
| `QuickMail/Assets/ContextImages/{light,dark}/social-1.jpg` | *Castles of Burgundy (Board Game).jpg* | Mshuang2 | CC0 1.0 (public domain dedication), https://creativecommons.org/publicdomain/zero/1.0/ | https://commons.wikimedia.org/wiki/File:Castles_of_Burgundy_(Board_Game).jpg | Cropped to 3.2:1 and resized to 960 × 300; light set blended 50% toward white, dark set 65% toward #202124; re-encoded as JPEG |
| `QuickMail/Assets/ContextImages/{light,dark}/social-2.jpg` | *Wine glasses on a box cutting board with cheese fruit and bread chalkboard and personalized pillows on a picnic blanket (19924590745).jpg* | Personal Creations | Creative Commons Attribution 2.0 (CC BY 2.0), https://creativecommons.org/licenses/by/2.0/ | https://commons.wikimedia.org/wiki/File:Wine_glasses_on_a_box_cutting_board_with_cheese_fruit_and_bread_chalkboard_and_personalized_pillows_on_a_picnic_blanket_(19924590745).jpg | Cropped to 3.2:1 and resized to 960 × 300; light set blended 50% toward white, dark set 65% toward #202124; re-encoded as JPEG |
| `QuickMail/Assets/ContextImages/{light,dark}/entertainment-1.jpg` | *Cinema Seats I (51830912465).jpg* | Eden, Janine and Jim from New York City | Creative Commons Attribution 2.0 (CC BY 2.0), https://creativecommons.org/licenses/by/2.0/ | https://commons.wikimedia.org/wiki/File:Cinema_Seats_I_(51830912465).jpg | Cropped to 3.2:1 and resized to 960 × 300; light set blended 50% toward white, dark set 65% toward #202124; re-encoded as JPEG |
| `QuickMail/Assets/ContextImages/{light,dark}/entertainment-2.jpg` | *Film reel. Kodachrome movie film roll cameras only Daylight type. Baugnez 44 Historical Center WW2 museum 2012.jpg* | nick@ | Creative Commons Attribution 2.0 (CC BY 2.0), https://creativecommons.org/licenses/by/2.0/ | https://commons.wikimedia.org/wiki/File:Film_reel._Kodachrome_movie_film_roll_cameras_only_Daylight_type._Baugnez_44_Historical_Center_WW2_museum_2012.jpg | Cropped to 3.2:1 and resized to 960 × 300; light set blended 50% toward white, dark set 65% toward #202124; re-encoded as JPEG |

## MailBox MatriX's own artwork

The application icon (`QuickMail/Assets/App`) is an original work drawn from code by
`Tools/MailBoXX.ArtGen` and is covered by MailBox MatriX's MIT licence. The same tool can still draw
placeholder row pictures (`--placeholder-context-art`); they are not shipped.
