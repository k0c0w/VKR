import { Room } from "./Room";

export interface Building {
  name: string;
  address: string;
  levels: Level[];
}

interface Level {
  name: string;
  rooms: Room[];
}
