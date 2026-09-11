# Economy balance

Classic-scale prices mapped to the existing Malapoly neighborhoods. Baseline: [Hasbro Monopoly rules](https://www.hasbro.com/common/instruct/Monopoly_Vintage.pdf).

- Starting cash: 1,500. A completed forward circuit pays 200; jail transfers pay nothing.
- Streets: 60–400; transports: 200; utilities: 150.
- Houses cost 50, 100, 150, or 200 by color group. After four houses, one additional building payment upgrades to a hotel.
- Unimproved streets charge double rent when the owner has the full color group.
- Transport rent: 25 / 50 / 100 / 200 for one / two / three / four owned.
- Utility rent: dice total ×4 for one utility, ×10 for both. Normal rolls use two six-sided dice.
- Income tax: 200. Super tax: 100. Mandatory rent/tax charges are protected against duplicate payment requests.
- Chance and Treasure currently use monetary cards only, with both rewards and fees.

## Win/loss rule

Cash below zero eliminates a player; zero cash remains playable. Properties and buildings return to the bank, eliminated players are skipped, and the last solvent player wins. Rent creditors receive only the cash the payer actually had. This is a simplified rule because mortgages, asset liquidation, and trading are not implemented. Building evenly across a group is not enforced. Hotels are shown in the purchase panel and charged correctly, but retain the current four-house artwork.

## Street prices and rents

| Property | Price | House / hotel upgrade | Base rent | 1 house | 2 houses | 3 houses | 4 houses | Hotel |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| Cabuz | 60 | 50 | 2 | 10 | 30 | 90 | 160 | 250 |
| Interseccion | 60 | 50 | 4 | 20 | 60 | 180 | 320 | 450 |
| San Isidro | 100 | 50 | 6 | 30 | 90 | 270 | 400 | 550 |
| El Rastro | 100 | 50 | 6 | 30 | 90 | 270 | 400 | 550 |
| Ineb | 120 | 50 | 8 | 40 | 100 | 300 | 450 | 600 |
| Estadio | 140 | 100 | 10 | 50 | 150 | 450 | 625 | 750 |
| Mercado | 140 | 100 | 10 | 50 | 150 | 450 | 625 | 750 |
| Bomberos | 160 | 100 | 12 | 60 | 180 | 500 | 700 | 900 |
| La Terminal | 180 | 100 | 14 | 70 | 200 | 550 | 750 | 950 |
| La Estrella | 180 | 100 | 14 | 70 | 200 | 550 | 750 | 950 |
| Parque | 200 | 100 | 16 | 80 | 220 | 600 | 800 | 1000 |
| La Municipalidad | 220 | 150 | 18 | 90 | 250 | 700 | 875 | 1050 |
| Starbus | 220 | 150 | 18 | 90 | 250 | 700 | 875 | 1050 |
| Farmacia Higia | 240 | 150 | 20 | 100 | 300 | 750 | 925 | 1100 |
| Chop suey | 260 | 150 | 22 | 110 | 330 | 800 | 975 | 1150 |
| Pollo Campero | 260 | 150 | 22 | 110 | 330 | 800 | 975 | 1150 |
| La Trinidad | 280 | 150 | 24 | 120 | 360 | 850 | 1025 | 1200 |
| Benson | 300 | 200 | 26 | 130 | 390 | 900 | 1100 | 1275 |
| Mangos | 300 | 200 | 26 | 130 | 390 | 900 | 1100 | 1275 |
| Santa Lucia | 320 | 200 | 28 | 150 | 450 | 1000 | 1200 | 1400 |
| Malacateco | 350 | 200 | 35 | 175 | 500 | 1100 | 1300 | 1500 |
| Bon Cafe | 400 | 200 | 50 | 200 | 600 | 1400 | 1700 | 2000 |

## Validation

The JSON and ScriptableObject values were checked for agreement, increasing street rents, valid building costs, and unique board IDs. Regression tests cover hotel upgrades, special-property rent, and bankruptcy. Unity test execution still needs to be run in the Editor; the batch-mode run was not approved.
