// What the shop unlocks at each level (the XP for each level lives in the API)
export type ShopLevel = {
  level: number
  title: string
  unlock: string
}

export const shopLevels: ShopLevel[] = [
  { level: 1, title: 'Coffee Cart', unlock: 'A little cart, an umbrella, and a French press' },
  { level: 2, title: 'Espresso Cart', unlock: 'A real espresso machine for the cart' },
  { level: 3, title: 'Walk-up Window', unlock: 'Your own shack with an ORDER HERE window' },
  { level: 4, title: 'Board Rack', unlock: 'Surfboards leaning on the wall' },
  { level: 5, title: 'Palm Patio', unlock: 'A palm tree and beach chairs out front' },
  { level: 6, title: 'String Lights', unlock: 'Lights strung along the roof' },
  { level: 7, title: 'Shop Dog', unlock: 'A shop dog napping in the sand' },
  { level: 8, title: 'Striped Awning', unlock: 'A striped awning over the window' },
  { level: 9, title: 'Neon Open', unlock: 'A glowing OPEN sign' },
  { level: 10, title: 'Beachfront Café', unlock: 'Sunshine over the water and your name on the roof' },
]

export function shopLevel(level: number): ShopLevel {
  return shopLevels[Math.min(Math.max(level, 1), shopLevels.length) - 1]
}

export function nextShopLevel(level: number): ShopLevel | null {
  return level < shopLevels.length ? shopLevels[level] : null
}
