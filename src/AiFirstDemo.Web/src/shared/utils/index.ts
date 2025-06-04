// Godfather quotes for loading states and theming
export const godfatherQuotes = [
  "I'm gonna make him an offer he can't refuse.",
  "It's not personal, Sonny. It's strictly business.",
  "Keep your friends close, but your enemies closer.",
  "A man who doesn't spend time with his family can never be a real man.",
  "Great men are not born great, they grow great.",
  "Never hate your enemies. It affects your judgment.",
  "Revenge is a dish best served cold.",
  "I don't like violence, Tom. I'm a businessman; blood is a big expense.",
  "In Sicily, women are more dangerous than shotguns.",
  "Time wounds all heels.",
  "Behind every successful fortune there is a crime.",
  "Finance is a gun. Politics is knowing when to pull the trigger.",
  "The lawyer with the briefcase can steal more money than the man with the gun.",
  "I have learned more in the streets than in any classroom.",
  "Friendship is everything. Friendship is more than talent. It is more than government.",
  "You can act like a man! What's the matter with you?",
  "Don't ever take sides with anyone against the family again.",
  "I spent my whole life trying not to be careless. Women and children can afford to be careless, but not men.",
  "It's the smart move. Tessio was always smarter.",
  "Leave the gun. Take the cannoli."
]

export const getRandomGodfatherQuote = (): string => {
  return godfatherQuotes[Math.floor(Math.random() * godfatherQuotes.length)]
}

// AI Pacino specific quotes for analytics
export const aiPacinoAnalyticsQuotes = [
  "Say hello to my little algorithm!",
  "The data never lies, but sometimes it whispers.",
  "In this business, you keep your numbers close.",
  "Every click tells a story, capisce?",
  "The analytics family always knows the truth.",
  "Numbers don't lie, but people do.",
  "The intelligence in the machine runs deep.",
  "Data flows like respect - it must be earned.",
  "The analytics don't sleep, and neither do we.",
  "Information is power, power is everything."
]

export const getRandomAIPacinoQuote = (): string => {
  return aiPacinoAnalyticsQuotes[Math.floor(Math.random() * aiPacinoAnalyticsQuotes.length)]
} 