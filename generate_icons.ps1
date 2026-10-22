$OutputDir = "C:\Users\Rasmus\Documents\Code projects\VikingSkills\assets\keystones"
if (!(Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}

Add-Type -AssemblyName System.Drawing

function Create-BaseEmblem([string]$theme) {
    $bmp = New-Object System.Drawing.Bitmap(128, 128)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.Clear([System.Drawing.Color]::FromArgb(0, 0, 0, 0))

    $rimColor = [System.Drawing.Color]::FromArgb(255, 90, 110, 140)
    $accentColor = [System.Drawing.Color]::FromArgb(255, 56, 189, 248)

    switch ($theme) {
        "sword" { $rimColor = [System.Drawing.Color]::FromArgb(255, 148, 163, 184); $accentColor = [System.Drawing.Color]::FromArgb(255, 226, 232, 240) }
        "blood" { $rimColor = [System.Drawing.Color]::FromArgb(255, 185, 28, 28); $accentColor = [System.Drawing.Color]::FromArgb(255, 239, 68, 68) }
        "axe"   { $rimColor = [System.Drawing.Color]::FromArgb(255, 217, 119, 6); $accentColor = [System.Drawing.Color]::FromArgb(255, 245, 158, 11) }
        "club"  { $rimColor = [System.Drawing.Color]::FromArgb(255, 202, 138, 4); $accentColor = [System.Drawing.Color]::FromArgb(255, 250, 204, 21) }
        "spear" { $rimColor = [System.Drawing.Color]::FromArgb(255, 13, 148, 136); $accentColor = [System.Drawing.Color]::FromArgb(255, 45, 212, 191) }
        "dagger"{ $rimColor = [System.Drawing.Color]::FromArgb(255, 126, 34, 206); $accentColor = [System.Drawing.Color]::FromArgb(255, 168, 85, 247) }
        "fist"  { $rimColor = [System.Drawing.Color]::FromArgb(255, 194, 65, 12); $accentColor = [System.Drawing.Color]::FromArgb(255, 249, 115, 22) }
        "bow"   { $rimColor = [System.Drawing.Color]::FromArgb(255, 21, 128, 61); $accentColor = [System.Drawing.Color]::FromArgb(255, 34, 197, 94) }
        "shield"{ $rimColor = [System.Drawing.Color]::FromArgb(255, 37, 99, 235); $accentColor = [System.Drawing.Color]::FromArgb(255, 96, 165, 250) }
        "magic_fire" { $rimColor = [System.Drawing.Color]::FromArgb(255, 194, 65, 12); $accentColor = [System.Drawing.Color]::FromArgb(255, 249, 115, 22) }
        "magic_frost" { $rimColor = [System.Drawing.Color]::FromArgb(255, 14, 116, 144); $accentColor = [System.Drawing.Color]::FromArgb(255, 56, 189, 248) }
        "magic_blood" { $rimColor = [System.Drawing.Color]::FromArgb(255, 153, 27, 27); $accentColor = [System.Drawing.Color]::FromArgb(255, 239, 68, 68) }
        "magic_spirit" { $rimColor = [System.Drawing.Color]::FromArgb(255, 161, 98, 247); $accentColor = [System.Drawing.Color]::FromArgb(255, 232, 121, 249) }
        "nature" { $rimColor = [System.Drawing.Color]::FromArgb(255, 4, 120, 87); $accentColor = [System.Drawing.Color]::FromArgb(255, 52, 211, 153) }
        "gold"  { $rimColor = [System.Drawing.Color]::FromArgb(255, 180, 83, 9); $accentColor = [System.Drawing.Color]::FromArgb(255, 251, 191, 36) }
    }

    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $path.AddEllipse(8, 8, 112, 112)
    $pt1 = New-Object System.Drawing.Point(64, 8)
    $pt2 = New-Object System.Drawing.Point(64, 120)
    $bgBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush($pt1, $pt2, [System.Drawing.Color]::FromArgb(255, 28, 35, 48), [System.Drawing.Color]::FromArgb(255, 9, 11, 16))
    $g.FillPath($bgBrush, $path)

    $innerGlow = New-Object System.Drawing.Drawing2D.PathGradientBrush($path)
    $innerGlow.CenterColor = [System.Drawing.Color]::FromArgb(45, $accentColor.R, $accentColor.G, $accentColor.B)
    $innerGlow.SurroundColors = @([System.Drawing.Color]::FromArgb(0, 0, 0, 0))
    $g.FillPath($innerGlow, $path)

    $rimPen = New-Object System.Drawing.Pen($rimColor, 3.5)
    $g.DrawEllipse($rimPen, 8, 8, 112, 112)

    $bevelPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(60, 255, 255, 255), 1.2)
    $g.DrawEllipse($bevelPen, 11, 11, 106, 106)

    return @{ Bitmap = $bmp; Graphics = $g; Accent = $accentColor; Rim = $rimColor }
}

function Save-Icon($ctx, [string]$name) {
    $outPath = Join-Path $OutputDir "$name.png"
    $ctx.Bitmap.Save($outPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $ctx.Graphics.Dispose()
    $ctx.Bitmap.Dispose()
    Write-Host "Created $name.png"
}

# 1. keystone_sword_blade
$c = Create-BaseEmblem "sword"
$g = $c.Graphics
$bladePen = New-Object System.Drawing.Pen([System.Drawing.Color]::White, 6.0)
$bladePen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
$bladePen.EndCap = [System.Drawing.Drawing2D.LineCap]::Triangle
$g.DrawLine($bladePen, 36, 92, 92, 36)
$edgePen = New-Object System.Drawing.Pen($c.Accent, 2.0)
$g.DrawLine($edgePen, 37, 91, 91, 37)
$guardPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 203, 213, 225), 4.0)
$guardPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
$guardPen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
$g.DrawLine($guardPen, 32, 80, 48, 96)
$g.FillEllipse((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 245, 158, 11))), 25, 97, 8, 8)
$g.FillEllipse((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)), 72, 52, 6, 6)
Save-Icon $c "keystone_sword_blade"

# 2. keystone_sword_flurry
$c = Create-BaseEmblem "sword"
$g = $c.Graphics
$p1 = New-Object System.Drawing.Pen([System.Drawing.Color]::White, 4.5)
$p1.EndCap = [System.Drawing.Drawing2D.LineCap]::Triangle
$g.DrawLine($p1, 28, 96, 96, 32)
$g.DrawLine($p1, 100, 96, 32, 32)
$pG = New-Object System.Drawing.Pen($c.Accent, 3.5)
$g.DrawLine($pG, 26, 84, 40, 98)
$g.DrawLine($pG, 102, 84, 88, 98)
$pSwirl = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(180, 56, 189, 248), 2.0)
$g.DrawArc($pSwirl, 26, 26, 76, 76, 30, 75)
$g.DrawArc($pSwirl, 26, 26, 76, 76, 210, 75)
Save-Icon $c "keystone_sword_flurry"

# 3. keystone_sword_bleed
$c = Create-BaseEmblem "blood"
$g = $c.Graphics
$pBlade = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 241, 245, 249), 5.0)
$pBlade.EndCap = [System.Drawing.Drawing2D.LineCap]::Triangle
$g.DrawLine($pBlade, 34, 94, 94, 34)
$pRed = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 239, 68, 68), 3.0)
$g.DrawLine($pRed, 35, 93, 80, 48)
$bBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 220, 38, 38))
$g.FillEllipse($bBrush, 62, 70, 8, 12)
$g.FillEllipse($bBrush, 78, 60, 6, 9)
$g.FillEllipse($bBrush, 48, 82, 7, 10)
$pG = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 148, 163, 184), 4.0)
$g.DrawLine($pG, 28, 82, 44, 98)
Save-Icon $c "keystone_sword_bleed"

# 4. keystone_axe_cleave
$c = Create-BaseEmblem "axe"
$g = $c.Graphics
$haftPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 180, 83, 9), 5.0)
$haftPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
$g.DrawLine($haftPen, 40, 100, 84, 28)
$axePath = New-Object System.Drawing.Drawing2D.GraphicsPath
$axePath.AddLines(@(
    (New-Object System.Drawing.Point(78, 32)),
    (New-Object System.Drawing.Point(104, 36)),
    (New-Object System.Drawing.Point(106, 56)),
    (New-Object System.Drawing.Point(82, 64)),
    (New-Object System.Drawing.Point(74, 52)),
    (New-Object System.Drawing.Point(74, 34))
))
$g.FillPath((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 226, 232, 240))), $axePath)
$g.DrawPath((New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 245, 158, 11), 2.0)), $axePath)
$arcPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(180, 245, 158, 11), 2.5)
$g.DrawArc($arcPen, 20, 20, 88, 88, 320, 90)
Save-Icon $c "keystone_axe_cleave"

# 5. keystone_axe_fury
$c = Create-BaseEmblem "axe"
$g = $c.Graphics
$haftPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 180, 83, 9), 5.0)
$g.DrawLine($haftPen, 64, 102, 64, 26)
$leftBlade = New-Object System.Drawing.Drawing2D.GraphicsPath
$leftBlade.AddLines(@(
    (New-Object System.Drawing.Point(64, 36)),
    (New-Object System.Drawing.Point(34, 32)),
    (New-Object System.Drawing.Point(30, 58)),
    (New-Object System.Drawing.Point(64, 54))
))
$g.FillPath((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 226, 232, 240))), $leftBlade)
$rightBlade = New-Object System.Drawing.Drawing2D.GraphicsPath
$rightBlade.AddLines(@(
    (New-Object System.Drawing.Point(64, 36)),
    (New-Object System.Drawing.Point(94, 32)),
    (New-Object System.Drawing.Point(98, 58)),
    (New-Object System.Drawing.Point(64, 54))
))
$g.FillPath((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 226, 232, 240))), $rightBlade)
$sparkBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 239, 68, 68))
$g.FillEllipse($sparkBrush, 24, 26, 6, 6)
$g.FillEllipse($sparkBrush, 98, 26, 6, 6)
$g.FillEllipse($sparkBrush, 61, 16, 6, 6)
Save-Icon $c "keystone_axe_fury"

# 6. keystone_axe_timber
$c = Create-BaseEmblem "nature"
$g = $c.Graphics
$logBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 120, 53, 15))
$g.FillRectangle($logBrush, 36, 76, 56, 26)
$g.DrawRectangle((New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 180, 83, 9), 2.0)), 36, 76, 56, 26)
$g.FillEllipse((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 217, 119, 6))), 56, 84, 16, 10)
$splitPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 251, 191, 36), 3.0)
$g.DrawLine($splitPen, 64, 76, 64, 94)
$haft = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 203, 213, 225), 4.0)
$g.DrawLine($haft, 44, 30, 72, 70)
$head = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 148, 163, 184))
$g.FillPolygon($head, @((New-Object System.Drawing.Point(64, 60)), (New-Object System.Drawing.Point(84, 66)), (New-Object System.Drawing.Point(76, 76))))
Save-Icon $c "keystone_axe_timber"

# 7. keystone_hammer_crush
$c = Create-BaseEmblem "club"
$g = $c.Graphics
$hPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 180, 83, 9), 5.0)
$g.DrawLine($hPen, 40, 96, 72, 44)
$hamBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 203, 213, 225))
$g.FillRectangle($hamBrush, 60, 28, 40, 24)
$g.DrawRectangle((New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 250, 204, 21), 2.0)), 60, 28, 40, 24)
$rPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(180, 250, 204, 21), 2.5)
$g.DrawArc($rPen, 24, 58, 80, 48, 20, 140)
$g.DrawArc($rPen, 16, 50, 96, 64, 25, 130)
Save-Icon $c "keystone_hammer_crush"

# 8. keystone_club_thunder
$c = Create-BaseEmblem "club"
$g = $c.Graphics
$maceBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 203, 213, 225))
$g.FillEllipse($maceBrush, 46, 26, 36, 36)
$spPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 250, 204, 21), 3.0)
$g.DrawLine($spPen, 64, 18, 64, 28)
$g.DrawLine($spPen, 40, 44, 48, 44)
$g.DrawLine($spPen, 80, 44, 88, 44)
$g.DrawLine($spPen, 48, 28, 54, 34)
$g.DrawLine($spPen, 80, 28, 74, 34)
$hPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 148, 163, 184), 5.0)
$g.DrawLine($hPen, 64, 62, 64, 104)
$bolt = New-Object System.Drawing.Drawing2D.GraphicsPath
$bolt.AddLines(@(
    (New-Object System.Drawing.Point(82, 34)),
    (New-Object System.Drawing.Point(74, 52)),
    (New-Object System.Drawing.Point(84, 52)),
    (New-Object System.Drawing.Point(72, 74)),
    (New-Object System.Drawing.Point(78, 58)),
    (New-Object System.Drawing.Point(70, 58))
))
$g.FillPath((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 56, 189, 248))), $bolt)
Save-Icon $c "keystone_club_thunder"

# 9. keystone_club_sunder
$c = Create-BaseEmblem "club"
$g = $c.Graphics
$sunderBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 148, 163, 184))
$g.FillEllipse($sunderBrush, 38, 36, 52, 56)
$cPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 239, 68, 68), 3.0)
$cPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
$cPen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
$g.DrawLines($cPen, @(
    (New-Object System.Drawing.Point(64, 36)),
    (New-Object System.Drawing.Point(58, 52)),
    (New-Object System.Drawing.Point(68, 66)),
    (New-Object System.Drawing.Point(60, 80)),
    (New-Object System.Drawing.Point(66, 92))
))
$g.FillEllipse((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 250, 204, 21))), 60, 48, 8, 8)
Save-Icon $c "keystone_club_sunder"

# 10. keystone_spear_thrust
$c = Create-BaseEmblem "spear"
$g = $c.Graphics
$sPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 180, 83, 9), 4.0)
$g.DrawLine($sPen, 32, 96, 80, 48)
$tip = New-Object System.Drawing.Drawing2D.GraphicsPath
$tip.AddLines(@(
    (New-Object System.Drawing.Point(76, 52)),
    (New-Object System.Drawing.Point(104, 24)),
    (New-Object System.Drawing.Point(94, 40)),
    (New-Object System.Drawing.Point(88, 34))
))
$g.FillPath((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 241, 245, 249))), $tip)
$g.DrawPath((New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 45, 212, 191), 2.0)), $tip)
$tPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(160, 45, 212, 191), 2.0)
$g.DrawLine($tPen, 40, 42, 60, 22)
$g.DrawLine($tPen, 50, 52, 70, 32)
Save-Icon $c "keystone_spear_thrust"

# 11. keystone_spear_lightning
$c = Create-BaseEmblem "spear"
$g = $c.Graphics
$sPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 56, 189, 248), 3.5)
$g.DrawLine($sPen, 30, 98, 76, 52)
$lHead = New-Object System.Drawing.Drawing2D.GraphicsPath
$lHead.AddLines(@(
    (New-Object System.Drawing.Point(72, 56)),
    (New-Object System.Drawing.Point(84, 44)),
    (New-Object System.Drawing.Point(80, 44)),
    (New-Object System.Drawing.Point(104, 20)),
    (New-Object System.Drawing.Point(92, 36)),
    (New-Object System.Drawing.Point(96, 36)),
    (New-Object System.Drawing.Point(76, 52))
))
$g.FillPath((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)), $lHead)
$g.DrawPath((New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 56, 189, 248), 2.0)), $lHead)
$g.FillEllipse((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)), 62, 38, 5, 5)
$g.FillEllipse((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)), 94, 52, 5, 5)
Save-Icon $c "keystone_spear_lightning"

# 12. keystone_polearm_sweep
$c = Create-BaseEmblem "spear"
$g = $c.Graphics
$haft = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 180, 83, 9), 4.0)
$g.DrawLine($haft, 36, 96, 76, 44)
$cres = New-Object System.Drawing.Drawing2D.GraphicsPath
$cres.AddArc(60, 20, 48, 48, 120, 160)
$cres.AddArc(68, 26, 36, 36, 280, -160)
$cres.CloseFigure()
$g.FillPath((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 226, 232, 240))), $cres)
$spin = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(180, 45, 212, 191), 2.5)
$g.DrawArc($spin, 24, 24, 80, 80, 20, 260)
Save-Icon $c "keystone_polearm_sweep"

# 13. keystone_dagger_ambush
$c = Create-BaseEmblem "dagger"
$g = $c.Graphics
$dag = New-Object System.Drawing.Drawing2D.GraphicsPath
$dag.AddLines(@(
    (New-Object System.Drawing.Point(64, 30)),
    (New-Object System.Drawing.Point(70, 56)),
    (New-Object System.Drawing.Point(64, 94)),
    (New-Object System.Drawing.Point(58, 56))
))
$g.FillPath((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 241, 245, 249))), $dag)
$g.DrawPath((New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 168, 85, 247), 2.0)), $dag)
$hPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 251, 191, 36), 3.5)
$g.DrawLine($hPen, 52, 54, 76, 54)
$g.DrawLine($hPen, 64, 54, 64, 26)
$g.FillEllipse((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)), 62, 80, 5, 5)
Save-Icon $c "keystone_dagger_ambush"

# 14. keystone_dagger_shadow
$c = Create-BaseEmblem "dagger"
$g = $c.Graphics
$cowl = New-Object System.Drawing.Drawing2D.GraphicsPath
$cowl.AddLines(@(
    (New-Object System.Drawing.Point(64, 26)),
    (New-Object System.Drawing.Point(92, 70)),
    (New-Object System.Drawing.Point(78, 88)),
    (New-Object System.Drawing.Point(64, 80)),
    (New-Object System.Drawing.Point(50, 88)),
    (New-Object System.Drawing.Point(36, 70))
))
$g.FillPath((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 30, 27, 75))), $cowl)
$g.DrawPath((New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 168, 85, 247), 2.0)), $cowl)
$eyeBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 192, 132, 252))
$g.FillEllipse($eyeBrush, 52, 54, 7, 5)
$g.FillEllipse($eyeBrush, 69, 54, 7, 5)
Save-Icon $c "keystone_dagger_shadow"

# 15. keystone_fist_iron
$c = Create-BaseEmblem "fist"
$g = $c.Graphics
$fist = New-Object System.Drawing.Drawing2D.GraphicsPath
$fist.AddLines(@(
    (New-Object System.Drawing.Point(46, 88)),
    (New-Object System.Drawing.Point(46, 52)),
    (New-Object System.Drawing.Point(54, 40)),
    (New-Object System.Drawing.Point(74, 40)),
    (New-Object System.Drawing.Point(82, 52)),
    (New-Object System.Drawing.Point(82, 88))
))
$g.FillPath((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 148, 163, 184))), $fist)
$g.DrawPath((New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 249, 115, 22), 2.5)), $fist)
$kBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 251, 191, 36))
$g.FillEllipse($kBrush, 52, 44, 6, 6)
$g.FillEllipse($kBrush, 61, 42, 6, 6)
$g.FillEllipse($kBrush, 70, 44, 6, 6)
Save-Icon $c "keystone_fist_iron"

# 16. keystone_fist_feral
$c = Create-BaseEmblem "fist"
$g = $c.Graphics
$sPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 239, 68, 68), 5.0)
$sPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Triangle
$sPen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
$g.DrawLine($sPen, 38, 38, 70, 92)
$g.DrawLine($sPen, 52, 28, 84, 82)
$g.DrawLine($sPen, 66, 38, 98, 92)
$cPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 254, 202, 202), 2.0)
$g.DrawLine($cPen, 39, 39, 69, 91)
$g.DrawLine($cPen, 53, 29, 83, 81)
$g.DrawLine($cPen, 67, 39, 97, 91)
Save-Icon $c "keystone_fist_feral"

# 17. keystone_bow_eagle
$c = Create-BaseEmblem "bow"
$g = $c.Graphics
$bPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 180, 83, 9), 4.0)
$g.DrawArc($bPen, 26, 26, 76, 76, 120, 120)
$strPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 226, 232, 240), 1.5)
$g.DrawLine($strPen, 34, 42, 70, 64)
$g.DrawLine($strPen, 34, 86, 70, 64)
$arrPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 34, 197, 94), 2.5)
$arrPen.EndCap = [System.Drawing.Drawing2D.LineCap]::ArrowAnchor
$g.DrawLine($arrPen, 70, 64, 100, 64)
Save-Icon $c "keystone_bow_eagle"

# 18. keystone_bow_arrow
$c = Create-BaseEmblem "bow"
$g = $c.Graphics
$aPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 226, 232, 240), 3.0)
$g.DrawLine($aPen, 32, 96, 90, 38)
$head = New-Object System.Drawing.Drawing2D.GraphicsPath
$head.AddLines(@(
    (New-Object System.Drawing.Point(86, 42)),
    (New-Object System.Drawing.Point(104, 24)),
    (New-Object System.Drawing.Point(92, 48))
))
$g.FillPath((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 34, 197, 94))), $head)
$fPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 34, 197, 94), 2.0)
$g.DrawLine($fPen, 36, 92, 30, 84)
$g.DrawLine($fPen, 42, 86, 36, 78)
Save-Icon $c "keystone_bow_arrow"

# 19. keystone_crossbow_crank
$c = Create-BaseEmblem "bow"
$g = $c.Graphics
$sPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 180, 83, 9), 6.0)
$g.DrawLine($sPen, 64, 98, 64, 38)
$prodPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 148, 163, 184), 4.5)
$g.DrawArc($prodPen, 24, 24, 80, 40, 180, 180)
$g.FillEllipse((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 251, 191, 36))), 56, 70, 16, 16)
$g.DrawEllipse((New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 180, 83, 9), 2.0)), 56, 70, 16, 16)
Save-Icon $c "keystone_crossbow_crank"

# 20. keystone_shield_bulwark
$c = Create-BaseEmblem "shield"
$g = $c.Graphics
$kite = New-Object System.Drawing.Drawing2D.GraphicsPath
$kite.AddLines(@(
    (New-Object System.Drawing.Point(34, 34)),
    (New-Object System.Drawing.Point(94, 34)),
    (New-Object System.Drawing.Point(88, 70)),
    (New-Object System.Drawing.Point(64, 98)),
    (New-Object System.Drawing.Point(40, 70))
))
$g.FillPath((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 30, 41, 59))), $kite)
$g.DrawPath((New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 96, 165, 250), 3.0)), $kite)
$g.FillEllipse((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 203, 213, 225))), 56, 48, 16, 16)
$g.DrawEllipse((New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 251, 191, 36), 2.0)), 56, 48, 16, 16)
Save-Icon $c "keystone_shield_bulwark"

# 21. keystone_shield_parry
$c = Create-BaseEmblem "shield"
$g = $c.Graphics
$g.FillEllipse((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 30, 41, 59))), 34, 34, 60, 60)
$g.DrawEllipse((New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 96, 165, 250), 3.0)), 34, 34, 60, 60)
$g.FillEllipse((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 203, 213, 225))), 54, 54, 20, 20)
$sparkPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 251, 191, 36), 3.0)
$g.DrawLine($sparkPen, 64, 40, 64, 22)
$g.DrawLine($sparkPen, 74, 46, 88, 34)
$g.DrawLine($sparkPen, 54, 46, 40, 34)
Save-Icon $c "keystone_shield_parry"

# 22. keystone_armor_plate
$c = Create-BaseEmblem "shield"
$g = $c.Graphics
$plate = New-Object System.Drawing.Drawing2D.GraphicsPath
$plate.AddLines(@(
    (New-Object System.Drawing.Point(42, 34)),
    (New-Object System.Drawing.Point(86, 34)),
    (New-Object System.Drawing.Point(92, 58)),
    (New-Object System.Drawing.Point(78, 92)),
    (New-Object System.Drawing.Point(50, 92)),
    (New-Object System.Drawing.Point(36, 58))
))
$g.FillPath((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 148, 163, 184))), $plate)
$g.DrawPath((New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 226, 232, 240), 2.5)), $plate)
$g.DrawLine((New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 203, 213, 225), 2.0)), 64, 36, 64, 90)
Save-Icon $c "keystone_armor_plate"

# 23. keystone_boot_wind
$c = Create-BaseEmblem "nature"
$g = $c.Graphics
$boot = New-Object System.Drawing.Drawing2D.GraphicsPath
$boot.AddLines(@(
    (New-Object System.Drawing.Point(46, 36)),
    (New-Object System.Drawing.Point(64, 36)),
    (New-Object System.Drawing.Point(64, 68)),
    (New-Object System.Drawing.Point(92, 74)),
    (New-Object System.Drawing.Point(90, 88)),
    (New-Object System.Drawing.Point(44, 88))
))
$g.FillPath((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 120, 53, 15))), $boot)
$g.DrawPath((New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 52, 211, 153), 2.0)), $boot)
$wing = New-Object System.Drawing.Drawing2D.GraphicsPath
$wing.AddLines(@(
    (New-Object System.Drawing.Point(44, 68)),
    (New-Object System.Drawing.Point(24, 52)),
    (New-Object System.Drawing.Point(32, 68)),
    (New-Object System.Drawing.Point(20, 62)),
    (New-Object System.Drawing.Point(40, 78))
))
$g.FillPath((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 52, 211, 153))), $wing)
Save-Icon $c "keystone_boot_wind"

# 24. keystone_jump_wings
$c = Create-BaseEmblem "gold"
$g = $c.Graphics
$lWing = New-Object System.Drawing.Drawing2D.GraphicsPath
$lWing.AddLines(@(
    (New-Object System.Drawing.Point(64, 78)),
    (New-Object System.Drawing.Point(26, 42)),
    (New-Object System.Drawing.Point(36, 62)),
    (New-Object System.Drawing.Point(26, 60)),
    (New-Object System.Drawing.Point(44, 82))
))
$g.FillPath((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 251, 191, 36))), $lWing)
$rWing = New-Object System.Drawing.Drawing2D.GraphicsPath
$rWing.AddLines(@(
    (New-Object System.Drawing.Point(64, 78)),
    (New-Object System.Drawing.Point(102, 42)),
    (New-Object System.Drawing.Point(92, 62)),
    (New-Object System.Drawing.Point(102, 60)),
    (New-Object System.Drawing.Point(84, 82))
))
$g.FillPath((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 251, 191, 36))), $rWing)
$g.FillPolygon((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)), @(
    (New-Object System.Drawing.Point(64, 26)),
    (New-Object System.Drawing.Point(70, 48)),
    (New-Object System.Drawing.Point(58, 48))
))
Save-Icon $c "keystone_jump_wings"

# 25. keystone_swim_wave
$c = Create-BaseEmblem "magic_frost"
$g = $c.Graphics
$wavePen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 56, 189, 248), 4.0)
$g.DrawArc($wavePen, 26, 48, 40, 40, 180, 180)
$g.DrawArc($wavePen, 62, 48, 40, 40, 180, 180)
$g.DrawArc($wavePen, 38, 32, 52, 52, 190, 180)
$fin = New-Object System.Drawing.Drawing2D.GraphicsPath
$fin.AddLines(@(
    (New-Object System.Drawing.Point(54, 74)),
    (New-Object System.Drawing.Point(64, 46)),
    (New-Object System.Drawing.Point(74, 74))
))
$g.FillPath((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 203, 213, 225))), $fin)
Save-Icon $c "keystone_swim_wave"

# 26. keystone_pickaxe_ore
$c = Create-BaseEmblem "gold"
$g = $c.Graphics
$pick = New-Object System.Drawing.Drawing2D.GraphicsPath
$pick.AddArc(30, 26, 68, 50, 190, 160)
$g.DrawPath((New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 203, 213, 225), 5.0)), $pick)
$hPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 180, 83, 9), 4.0)
$g.DrawLine($hPen, 64, 38, 64, 96)
$gem = New-Object System.Drawing.Drawing2D.GraphicsPath
$gem.AddLines(@(
    (New-Object System.Drawing.Point(64, 62)),
    (New-Object System.Drawing.Point(76, 72)),
    (New-Object System.Drawing.Point(64, 88)),
    (New-Object System.Drawing.Point(52, 72))
))
$g.FillPath((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 56, 189, 248))), $gem)
$g.DrawPath((New-Object System.Drawing.Pen([System.Drawing.Color]::White, 1.5)), $gem)
Save-Icon $c "keystone_pickaxe_ore"

# 27. keystone_wood_pine
$c = Create-BaseEmblem "nature"
$g = $c.Graphics
$b1 = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 5, 150, 105))
$g.FillPolygon($b1, @((New-Object System.Drawing.Point(64, 26)), (New-Object System.Drawing.Point(80, 50)), (New-Object System.Drawing.Point(48, 50))))
$g.FillPolygon($b1, @((New-Object System.Drawing.Point(64, 44)), (New-Object System.Drawing.Point(86, 68)), (New-Object System.Drawing.Point(42, 68))))
$g.FillPolygon($b1, @((New-Object System.Drawing.Point(64, 62)), (New-Object System.Drawing.Point(92, 88)), (New-Object System.Drawing.Point(36, 88))))
$g.FillRectangle((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 120, 53, 15))), 60, 88, 8, 14)
Save-Icon $c "keystone_wood_pine"

# 28. keystone_magic_fire
$c = Create-BaseEmblem "magic_fire"
$g = $c.Graphics
$flame = New-Object System.Drawing.Drawing2D.GraphicsPath
$flame.AddBezier(64, 24, 76, 42, 88, 58, 88, 76)
$flame.AddBezier(88, 76, 88, 88, 76, 98, 64, 98)
$flame.AddBezier(64, 98, 52, 98, 40, 88, 40, 76)
$flame.AddBezier(40, 76, 40, 58, 52, 42, 64, 24)
$g.FillPath((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 239, 68, 68))), $flame)
$core = New-Object System.Drawing.Drawing2D.GraphicsPath
$core.AddBezier(64, 46, 72, 60, 78, 70, 78, 80)
$core.AddBezier(78, 80, 78, 88, 72, 94, 64, 94)
$core.AddBezier(64, 94, 56, 94, 50, 88, 50, 80)
$core.AddBezier(50, 80, 50, 70, 56, 60, 64, 46)
$g.FillPath((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 251, 191, 36))), $core)
Save-Icon $c "keystone_magic_fire"

# 29. keystone_magic_frost
$c = Create-BaseEmblem "magic_frost"
$g = $c.Graphics
$sPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 56, 189, 248), 3.0)
$sPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
$sPen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
$g.DrawLine($sPen, 64, 26, 64, 102)
$g.DrawLine($sPen, 31, 45, 97, 83)
$g.DrawLine($sPen, 31, 83, 97, 45)
$bPen = New-Object System.Drawing.Pen([System.Drawing.Color]::White, 2.0)
$g.DrawLine($bPen, 64, 38, 56, 32); $g.DrawLine($bPen, 64, 38, 72, 32)
$g.DrawLine($bPen, 64, 90, 56, 96); $g.DrawLine($bPen, 64, 90, 72, 96)
$g.FillEllipse((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)), 60, 60, 8, 8)
Save-Icon $c "keystone_magic_frost"

# 30. keystone_magic_blood
$c = Create-BaseEmblem "magic_blood"
$g = $c.Graphics
$rPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 220, 38, 38), 2.5)
$g.DrawEllipse($rPen, 28, 28, 72, 72)
$drop = New-Object System.Drawing.Drawing2D.GraphicsPath
$drop.AddBezier(64, 34, 76, 54, 82, 68, 82, 78)
$drop.AddBezier(82, 78, 82, 88, 74, 94, 64, 94)
$drop.AddBezier(64, 94, 54, 94, 46, 88, 46, 78)
$drop.AddBezier(46, 78, 46, 68, 52, 54, 64, 34)
$g.FillPath((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 239, 68, 68))), $drop)
$g.FillEllipse((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 254, 202, 202))), 56, 66, 6, 8)
Save-Icon $c "keystone_magic_blood"

# 31. keystone_magic_spirit
$c = Create-BaseEmblem "magic_spirit"
$g = $c.Graphics
$pts = @(
    (New-Object System.Drawing.Point(64, 22)), (New-Object System.Drawing.Point(69, 52)),
    (New-Object System.Drawing.Point(98, 40)), (New-Object System.Drawing.Point(76, 64)),
    (New-Object System.Drawing.Point(106, 64)), (New-Object System.Drawing.Point(76, 69)),
    (New-Object System.Drawing.Point(98, 88)), (New-Object System.Drawing.Point(69, 76)),
    (New-Object System.Drawing.Point(64, 106)), (New-Object System.Drawing.Point(59, 76)),
    (New-Object System.Drawing.Point(30, 88)), (New-Object System.Drawing.Point(52, 69)),
    (New-Object System.Drawing.Point(22, 64)), (New-Object System.Drawing.Point(52, 64)),
    (New-Object System.Drawing.Point(30, 40)), (New-Object System.Drawing.Point(59, 52))
)
$g.FillPolygon((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 232, 121, 249))), $pts)
$g.FillEllipse((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)), 58, 58, 12, 12)
Save-Icon $c "keystone_magic_spirit"

# 32. keystone_eitr_crystal
$c = Create-BaseEmblem "magic_frost"
$g = $c.Graphics
$crys = New-Object System.Drawing.Drawing2D.GraphicsPath
$crys.AddLines(@(
    (New-Object System.Drawing.Point(64, 26)),
    (New-Object System.Drawing.Point(86, 64)),
    (New-Object System.Drawing.Point(64, 102)),
    (New-Object System.Drawing.Point(42, 64))
))
$g.FillPath((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 56, 189, 248))), $crys)
$g.DrawPath((New-Object System.Drawing.Pen([System.Drawing.Color]::White, 2.0)), $crys)
$oPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(160, 232, 121, 249), 2.0)
$g.DrawEllipse($oPen, 26, 48, 76, 32)
Save-Icon $c "keystone_eitr_crystal"

# 33. keystone_crit_strike
$c = Create-BaseEmblem "blood"
$g = $c.Graphics
$cPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 239, 68, 68), 2.5)
$g.DrawEllipse($cPen, 34, 34, 60, 60)
$g.DrawLine($cPen, 64, 22, 64, 44)
$g.DrawLine($cPen, 64, 84, 64, 106)
$g.DrawLine($cPen, 22, 64, 44, 64)
$g.DrawLine($cPen, 84, 64, 106, 64)
$burst = New-Object System.Drawing.Drawing2D.GraphicsPath
$burst.AddLines(@(
    (New-Object System.Drawing.Point(64, 52)),
    (New-Object System.Drawing.Point(76, 64)),
    (New-Object System.Drawing.Point(64, 76)),
    (New-Object System.Drawing.Point(52, 64))
))
$g.FillPath((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 251, 191, 36))), $burst)
$g.FillEllipse((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)), 62, 62, 4, 4)
Save-Icon $c "keystone_crit_strike"

# 34. keystone_stamina_flow
$c = Create-BaseEmblem "nature"
$g = $c.Graphics
$tPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 52, 211, 153), 4.0)
$tPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
$g.DrawArc($tPen, 40, 30, 48, 48, 0, 240)
$g.DrawArc($tPen, 30, 48, 48, 48, 120, 240)
$g.DrawArc($tPen, 50, 48, 48, 48, 240, 240)
$g.FillEllipse((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)), 61, 61, 6, 6)
Save-Icon $c "keystone_stamina_flow"

# 35. keystone_health_heart
$c = Create-BaseEmblem "blood"
$g = $c.Graphics
$heart = New-Object System.Drawing.Drawing2D.GraphicsPath
$heart.AddBezier(64, 46, 56, 32, 40, 32, 38, 48)
$heart.AddBezier(38, 48, 38, 64, 52, 78, 64, 94)
$heart.AddBezier(64, 94, 76, 78, 90, 64, 90, 48)
$heart.AddBezier(90, 48, 90, 32, 72, 32, 64, 46)
$g.FillPath((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 239, 68, 68))), $heart)
$g.DrawPath((New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 254, 202, 202), 2.0)), $heart)
$cPen = New-Object System.Drawing.Pen([System.Drawing.Color]::White, 3.0)
$g.DrawLine($cPen, 64, 52, 64, 72)
$g.DrawLine($cPen, 54, 62, 74, 62)
Save-Icon $c "keystone_health_heart"

# 36. keystone_fish_hook
$c = Create-BaseEmblem "magic_frost"
$g = $c.Graphics
$hPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 203, 213, 225), 4.0)
$g.DrawLine($hPen, 64, 26, 64, 68)
$g.DrawArc($hPen, 40, 52, 36, 36, 0, 180)
$g.DrawLine($hPen, 40, 64, 40, 54)
$g.DrawLine($hPen, 40, 54, 46, 60)
$g.DrawEllipse((New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 251, 191, 36), 2.0)), 61, 24, 6, 6)
Save-Icon $c "keystone_fish_hook"

Write-Host "Successfully generated all 36 keystone icons."
