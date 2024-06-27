// This source code is subject to the terms of the Mozilla Public License 2.0 at https://mozilla.org/MPL/2.0/
// © DevLucem

//@version=5

//
//       THIS CODE IS BASED FROM THE MT4 ZIGZAG INDICATOR
//       THE ZIGZAG SETTINGS FOR THE MAIN ONE ON TRADINGVIEW DO NOT WORK THE SAME AS MT4
//       I HOPE U LOVE IT
//

// By using this script, Please leave a comment to your result down below for i to see new ideas.
// And remember to ⛔⛔⛔ include link to this script on your write up. Always credit where credit is deserved,⛔⛔⛔ thanks for the love

indicator('ZigZag - lucemanb', overlay=true)

// inputs
Depth = input(12, title='Depth')  // Depth
Deviation = input(5, title='Deviation')  // Deviation

// ZigZag
var lastlow = 0.0
var lasthigh = 0.0




//x = ta.lowest(Depth)  => GIA THAP NHAT TRONG KHOANG DEPTH
//y = lastlow
//z = low => GIA THAP NHAT HIEN TAI
//a = Deviation
getLow(x, y, z, a) =>
    lastlow1 = y
    v = x
    m = v == lastlow1 or z - v > a * syminfo.mintick 
    if v != lastlow1
        lastlow1 := v
    if m
        v := 0.0
        v
    [v, lastlow1]
	

	
//x = ta.highest(Depth) => GIA CAO NHAT TRONG KHOANG DEPTH
//y = lasthigh
//z = high => GIA CAO NHAT HIEN TAI
//a = Deviation
getHigh(x, y, z, a) =>
    lasthigh1 = y
    v = x
    m = v == lasthigh1 or v - z > a * syminfo.mintick
    if v != lasthigh1
        lasthigh1 := v
    if m
        v := 0.0
        v
    [v, lasthigh1]

[v, e] = getLow(ta.lowest(Depth), lastlow, low, Deviation)
lastlow := e
zBB = v != 0.0
[v1, e1] = getHigh(ta.highest(Depth), lasthigh, high, Deviation)
lasthigh := e1
zSS = v1 != 0.0

zigzagDirection = -1
zigzagHigh = 0
zigzagLow = 0
zigzagDirection := zBB ? 0 : zSS ? 1 : nz(zigzagDirection[1], -1)
virtualLow = zigzagLow[1] + 1
if not zBB or zBB and zigzagDirection == zigzagDirection[1] and low > low[virtualLow]
    zigzagLow := nz(zigzagLow[1]) + 1
    zigzagLow
virtualHigh = zigzagHigh[1] + 1
if not zSS or zSS and zigzagDirection == zigzagDirection[1] and high < high[virtualHigh]
    zigzagHigh := nz(zigzagHigh[1]) + 1
    zigzagHigh
line zigzag = line.new(bar_index - zigzagLow, low[zigzagLow], bar_index - zigzagHigh, high[zigzagHigh], color=color.red, style=line.style_solid, width=2)
if zigzagDirection == zigzagDirection[1]
    line.delete(zigzag[1])

