const _=require("lodash");
const config=require("../config.json");
const { Pool }=require("pg");
const pool=new Pool({
	                    user: config.user,
	                    host: config.host,
	                    database: config.database,
	                    password: config.password,
	                    port: config.port,
	                    ssl: {
		                    rejectUnauthorized: false
	                    }
                    });

function _query(query, values=[]) {
	return new Promise((resolve, reject) => {
		pool.query(query, values, (err, res) => err ? reject(err) : resolve(res.rows));
	});
}

function _getPlayer(id) {
	return _query("SELECT * FROM players WHERE id=$1 LIMIT 1;", [id])
		.then((players) => players[0]);
}

function _createPlayer(id, name) {
	return _query("INSERT INTO players(id, name) VALUES ($1, $2) RETURNING *;", [id, name])
		.then((players) => players[0]);
}

function _updateGamesPlayed(player1, player2) {
	return _query(`UPDATE players
                   SET games=games + 1
                   WHERE id IN ($1, $2)`, [player1, player2]);
}

function _topN(top) {
	return _query(`SELECT l.score, p1.name AS player1, p2.name AS player2
                   FROM leaderboard l
                            JOIN players p1 ON l.player1 = p1.id
                            JOIN players p2 ON l.player2 = p2.id
                   ORDER BY score DESC
                   LIMIT $1;`, [top]);
}

function _topRelative(id) {
	return _query(`SELECT l.id,
                          l.score,
                          p1.name AS player1,
                          p2.name AS player2,
                          RANK() OVER (
                              ORDER BY score DESC
                              )      rank
                   FROM leaderboard l
                            JOIN players p1 ON l.player1 = p1.id
                            JOIN players p2 ON l.player2 = p2.id`)
		.then((ranks) => {
			let me;
			for(let rank of ranks) {
				if(rank.id === id) {
					me=rank;
					break;
				}
			}
			const index=_.findIndex(ranks, (rank) => rank.id === id);

			const above5=[];
			for(let i=index - 5; i < index && i < ranks.length - 1; i++) {
				if(!ranks[i]) continue;
				above5.push(ranks[i]);
			}

			const bottom5=[];
			for(let i=index + 5; i > index && i >= 0; i--) {
				if(!ranks[i]) continue;
				bottom5.push(ranks[i]);
			}

			return { me: me, above5: above5, bottom5: _.reverse(bottom5) };
		});
}

function _top5() {
	return _topN(5);
}

function _top10() {
	return _topN(10);
}

function _saveScore(score, player1, player2) {
	return _query("SELECT * FROM leaderboard WHERE score=$1 AND player1=$2 AND player2=$3;", [score, player1, player2])
		.then((result) => {
			if(result.length) return result;
			return _updateGamesPlayed(player1, player2)
				.then(() => _query("INSERT INTO leaderboard(score, player1, player2) VALUES ($1, $2, $3) RETURNING *;", [score, player1, player2]));
		})
		.then((scores) => scores[0])
		.then((score) => _topRelative(score.id, score.score));
}

module.exports={
	getPlayer: _getPlayer,
	createPlayer: _createPlayer,
	top5: _top5,
	top10: _top10,
	saveScore: _saveScore
};
